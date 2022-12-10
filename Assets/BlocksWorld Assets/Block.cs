using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    public ASP_Script asp_manager;
    public string Location;
    //private List<BoxCollider> boxColliders;
    private BoxCollider bottomCollider;
    bool changingLocation;
    // Start is called before the first frame update
    void Start()
    {
        /*
        //Find the bottom most collider
        BoxCollider tmpCollider = this.GetComponent<BoxCollider>();
        foreach (BoxCollider collider in this.GetComponentsInChildren<BoxCollider>())
        {
            if (collider.transform.position.y < tmpCollider.transform.position.y)
            {
                tmpCollider = collider;
            }

        }
        bottomCollider = tmpCollider;
        */
    }

    private void OnCollisionStay(Collision collision)
    {
        if(collision.gameObject.transform.position.y < this.transform.position.y)
        {
            if (!Location.Equals(collision.gameObject.name) && !changingLocation)
            {
                //Location changed
                StartCoroutine(ChangeLocationRoutine(collision));

            }
        }
    }

    IEnumerator ChangeLocationRoutine(Collision collision)
    {
        changingLocation = true;
        Location = collision.gameObject.name;
        asp_manager.ObserveState(); //Trigger Update world state
        yield return new WaitForSeconds(1); ; //wait one second to prevent bug
        changingLocation = false;
    }
}
