using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class Land : MonoBehaviour
{
    [SerializeField] double latitude;
    [SerializeField] double longitude;
    [SerializeField] Transform ObjectMarker;
    [SerializeField] bool ChangePos;
    Vector3 markerPos;
    Vector3 originPos = new Vector3(0,0,0);



    private void positionLand()
    {
        double latitude_rad = latitude * math.PI / 180;
        double longitude_rad = longitude * math.PI / 180;

        double zPos = -0.001 / 2 * math.cos(latitude_rad) * math.cos(longitude_rad);
        double xPos = 0.001 / 2 * math.cos(latitude_rad) * math.sin(longitude_rad);
        double yPos = 0.001 / 2 * math.sin(latitude_rad);

        markerPos.x = (float)xPos;
        markerPos.y = (float)yPos;
        markerPos.z = (float)zPos;

        ObjectMarker.position = markerPos;

        // In this case, the normal is more useful!
        Vector3 groundNormal = ObjectMarker.transform.position - originPos;
        // Here we work out what direction should be pointing forwards.
        Vector3 forwardsVector = -Vector3.Cross(groundNormal, ObjectMarker.transform.right);
        // Finally, compose the two directions back into a single rotation.
        gameObject.transform.rotation = Quaternion.LookRotation(forwardsVector, groundNormal);
        gameObject.transform.position = new Vector3((float)xPos, (float)yPos, (float)zPos);
    }



    // Start is called before the first frame update
    void Start()
    {
        ChangePos = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (ChangePos == true)
        {
            ChangePos = false;
            positionLand();
        }
    }
}
