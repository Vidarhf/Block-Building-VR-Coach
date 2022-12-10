using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

using System;
using System.IO;
using System.Diagnostics;
using TMPro;

public class ASP_Script : MonoBehaviour
{
    String ASP_InputFilePath = Environment.CurrentDirectory + @"\Assets\ASP_Assets\ASP_Encodings\ASP_InputStore.plan";
    String ASP_KnowledgeBaseFilePath = Environment.CurrentDirectory + @"\Assets\ASP_Assets\ASP_Encodings\ASP_KnowledgeBase.dl";
    String ASP_PlanFilePath = Environment.CurrentDirectory + @"\Assets\ASP_Assets\ASP_Encodings\ASP_Plan.plan";
    [SerializeField] private TMP_Text tmp_outputText;
    [SerializeField] private int PlanLength;

    [SerializeField] private List<GameObject> boxesObjects;
    [SerializeField] private AudioSource PlayerAudio;
    public bool goalStateReached = false;
    string message = "";
    private List<BoxCollider> BottomcColliders;
    private Collider a_collider;
    private Collider b_collider;
    private Collider c_colldier;

    // Goal state
    private Dictionary<string, string> goal_state = new Dictionary<string, string>();
            

    private Dictionary<string,string> world_state = new Dictionary<string, string>();
    private String[] actions;
    void Start()
    {
        
        // Set goal state dynamically
        goal_state = GetInitialGoalState();
        /*goal_state = new Dictionary<string, string>
            {
				
				// Example:   goal: on(c,b),on(b,a),on(a,table)? (3) 
				
                { "a",  "table" },
                { "b",  "a"     },
                { "c",  "b"     }
            };*/
        //Get current world state dynamically
        //world_state = GetWorldState();
        ObserveState();
        /*
        // Current state
        world_state = new Dictionary<string, string>
        {
            
				// Example:   on(a,table). on(b,table). on(c,a).
				
                { "a",  a_pos},
                { "b",  b_pos    },
                { "c",  c_pos    }
            };*/
        String factsCurrent = WorldToFacts(world_state);
        WriteKnowledgeBase(WorldToKnowledgebase());
        UpdateTextOutput(factsCurrent);
        message = factsCurrent;
    }

    private Dictionary<string, string> GetInitialGoalState()
    {
        Dictionary<string, string> newGoalState = new Dictionary<string, string>();


        //Iterate all boxes, create new pair with box and previous box as location.
        string lastLocation = "table"; //First box's location is table
        foreach(GameObject block in boxesObjects)
        {
            newGoalState.Add(block.name, lastLocation);
            lastLocation = block.name;
        }
        return newGoalState;
    }
    public void SetGoalStateToWorldState()
    {
        goal_state = world_state;
        TriggerGoalState(true);
    }


    /* 
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            ObserveState();
        }
    }
    */
    private GUIStyle guiStyle = new GUIStyle();

    void OnGUI()
    {
        guiStyle.fontSize = 20;
        GUI.Label(new Rect(20, 20, 300, 70), message, guiStyle);
        
    }

    public void UpdateTextOutput(string message)
    {
        if (tmp_outputText != null)
        {
            tmp_outputText.text += "\n" + message;
        }
    }

    /*
     * Observes the current state of world, compares it to goal and output actions
     */
    public void ObserveState()
    {
        world_state = GetWorldState();

        actions = GetPlan(world_state, goal_state);

        // Write the redirected output to this application's window.
        UnityEngine.Debug.Log("STATE ACTIONS" + actions.Length);
        UnityEngine.Debug.Log("-------------------------");
        
        if (actions.Length > 0 && actions[0].Contains("move") || actions[0].Contains("action")) 
        {
            //Check if goalstate is reached
            if (CompareX(world_state, goal_state))
            {
                  
                    TriggerGoalState(true);
                    message = "GOAL STATE REACHED";
                    
            }
            else
            {
                if (goalStateReached)
                {
                    //We've lost goalstate
                    TriggerGoalState(false);
                }
                message = "PLAN: ";
                for (int i = 0; i < actions.Length; i++)
                {
                    String action = actions[i];
                    UnityEngine.Debug.Log("Action: " + action);
                    if (action.Length > 4)
                    {
                        message += action + ", ";
                    }


                    /*
                    if (action.Contains("move"))   {

                        action = action.Replace("move(", "");
                        action = action.Replace(")", "");

                        String[] actionParts = action.Split(',');
                        string element_0 = actionParts[0];
                        string element_1 = actionParts[1];

                        // Do operation in the 3D view, etc.
                    }*/
                }
            }
   
            
            
        }
        UnityEngine.Debug.Log("-------------------------");
        UpdateTextOutput(message);
    }

  
    public bool CompareX<TKey, TValue>(Dictionary<TKey, TValue> dict1, Dictionary<TKey, TValue> dict2)
        {
            if (dict1 == dict2) return true;
            if ((dict1 == null) || (dict2 == null)) return false;
            if (dict1.Count != dict2.Count) return false;

            var valueComparer = EqualityComparer<TValue>.Default;

            foreach (var kvp in dict1)
            {
                TValue value2;
                if (!dict2.TryGetValue(kvp.Key, out value2)) return false;
                if (!valueComparer.Equals(kvp.Value, value2)) return false;
            }
            return true;
        }

    public string GetNextAction()
    {
        try
        {
            string nextAction = "";
            for (int i = 0; i < actions.Length; i++)
            {
                nextAction += actions[i].Replace(",", " on top of ");
                if(i + 1 != actions.Length)
                {
                    nextAction += " then ";
                }

            }
                 //= actions[actions.Length-2].Replace(",", " on top of ") + " then " + actions[actions.Length-1].Replace(",", " on top of ");
            return nextAction;
        }
        catch (Exception)
        {
            return ("Error retrieving next action");
            throw;
        }
        

        
    }

    public string GetGoalStateMessage()
    {
        string goalStateMessage = "The goal state is:";
        foreach(KeyValuePair<string, string> pair in goal_state)
        {
            goalStateMessage += " " + pair.Key + " on top of " + pair.Value + ",";
        }
        //goalStateMessage.Replace("a", "red");
        //goalStateMessage.Replace("b", "green");
        //goalStateMessage.Replace("c", "blue");
        return goalStateMessage;
    }

    //Trigger for goal state
    private void TriggerGoalState(bool v)
    {
        UnityEngine.Debug.Log("TriggerGoalState: " + v.ToString());
        goalStateReached = v;
        if (v)
        {
            //Goalstate has been reached play feedback
            PlayerAudio.Play();
        }
        if(!v)
        {
            //Goalstate lost
            PlayerAudio.Stop();
        }
    }

    private Dictionary<string, string> GetWorldState()
    {
        var currentWorldState = new Dictionary<string, string>();
        //Check all boxes for their current location
        foreach (GameObject box in boxesObjects)
        {
            currentWorldState.Add(box.name, box.GetComponent<Block>().Location);
        }

        return currentWorldState;
    }

    public String[] GetPlan(Dictionary<string, string> world_state, Dictionary<string, string> goal_state)
    {
        // Register the state
        RegisterCurrentState(world_state, goal_state);

        // Get Actions / Answer Set / Use option -N to specify the number of blocks, eg.N=4 means 5 blocks
        String answerSet = GetDLVResponse("-FP -N=" + (boxesObjects.Count-1), "");
        print("GetPlanDlv:" + answerSet);
        answerSet = answerSet.Replace("PLAN:", ">");
        String[] answerSetParts = answerSet.Split('>');
        try
        {
            answerSet = answerSetParts[1];
        }
        catch(IndexOutOfRangeException e) 
        { 
            
        }

        // Remove text that asks to check for alternative plans.
        int indexOfSteam = answerSet.IndexOf("Check");
        if (indexOfSteam >= 0)
            answerSet = answerSet.Remove(indexOfSteam);

        // Get the individual actions out from the AnswerSet String.
        answerSet = answerSet.Trim();
        String[] actions = answerSet.Split(';');

        return actions;
    }

    /**
     *
     */
    private string WorldToKnowledgebase()
    {
        String knowledgeBaseText = "";
        knowledgeBaseText += "location(table) :- true. true.";
        knowledgeBaseText += Environment.NewLine;
        knowledgeBaseText += Environment.NewLine;
        knowledgeBaseText += "location(B) :- block(B)."; //Location and block is not the same
        knowledgeBaseText += Environment.NewLine;
        knowledgeBaseText += Environment.NewLine;
        foreach (GameObject box in boxesObjects)
        {

            knowledgeBaseText += "block(" + box.name + "). ";
            
        }

        return (knowledgeBaseText);
    }

    private void WriteKnowledgeBase(string knowledgeBaseText)
    {
        using (System.IO.StreamWriter file =
            new System.IO.StreamWriter(ASP_KnowledgeBaseFilePath, false))
        {
            file.WriteLine(knowledgeBaseText);
        }
    }

    /** @function RegisterCurrentState | Register the current state into InputStore (file)
     *  @params Dictionary<string, string> world_state
     *  @return void
     **/
    void RegisterCurrentState(Dictionary<string, string> world_state, Dictionary<string, string> goal_state)
    {
        // Convert state dictionary to string.
        String factsCurrent = WorldToFacts(world_state);
        String factsGoal = GoalToFacts(goal_state);

        String facts = factsCurrent + factsGoal;

        // Append new state string to the AI_input file.
        using (System.IO.StreamWriter file =
            new System.IO.StreamWriter(ASP_InputFilePath, false))
        {
            file.WriteLine(facts);
        }
    }

    /**
     * @function GetDLVResponse | Contacts DLV solver with the current InputStore and KnowledgeBase. 
     * * @params String dlv_frontend, String dlv_options | These are settings for the DLV solver.
     * * @return String Answer set.
     */
    String GetDLVResponse(String dlv_frontend, String dlv_options)
    {
        // Set -silent by default.
        dlv_options = "-silent " + dlv_options; 

        // Connect to DLV Solver.
        Process process = new Process();
            process.StartInfo.FileName = Environment.CurrentDirectory + @"\Assets\ASP_Assets\ASP_Library\dlv.exe";
            process.StartInfo.Arguments = dlv_frontend +" "+ dlv_options +" "+ ASP_PlanFilePath + " " + ASP_InputFilePath + " " + ASP_KnowledgeBaseFilePath;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.Start();

            // Synchronously read the standard output of the spawned process. 
            StreamReader reader = process.StandardOutput;
            String output = reader.ReadToEnd();
            process.WaitForExit();

            return output; 
    }

    /**
     * @function WorldToFacts
     * @params Dictionary String, String world_state | state of the world
     * @return String facts | facts for ASP
     */
    public String WorldToFacts(Dictionary<String, String> world_state)
    {
        String facts = "";
        facts += "initially: ";
        facts += Environment.NewLine;

        foreach (KeyValuePair<string, string> fluent in world_state)
        {
            facts += "on(" + fluent.Key + "," + fluent.Value + "). ";
            facts += Environment.NewLine;              
        }

        //facts += "noConcurrency. ";
        facts += Environment.NewLine;
        facts += Environment.NewLine;

        return facts;
    }
	
    /**
     * @function GoalToFacts
     * @params Dictionary String, String goal_state
     * @return String facts | facts for ASP
     */
    public String GoalToFacts(Dictionary<String, String> goal_state)
    {
        String facts = "";

        facts += "goal: ";
        facts += Environment.NewLine;

        foreach (KeyValuePair<string, string> fluent in goal_state)
        {

            facts += "on(" + fluent.Key + "," + fluent.Value + "),";

        }

        facts = facts.TrimEnd(',');
        if (PlanLength != 0)
        {
            facts += "? (" + PlanLength + ")";
        }
        else
        {
            facts += "? (" + boxesObjects.Count + ")"; //Number corresponds to planlength (boxesObjects.Count)
        }
        

        return facts;
    }	
}

