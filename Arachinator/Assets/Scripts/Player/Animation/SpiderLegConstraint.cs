using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderLegConstraint : MonoBehaviour
{
    [SerializeField] GameObject moveCube; // the move cube of this leg
    [SerializeField] float legMoveSpeed = 7f; // the speed of the leg
    [SerializeField] float moveDistance = 0.7f; // how far does the cube have to go before the leg has to move to it
    [SerializeField] float moveStoppingDistance = 0.4f; // how close the leg should get to the cube before stopping
    [SerializeField] float upPosition = .3f;
    [SerializeField] SpiderLegConstraint oppsiteLeg; //the oppsite leg of this one

    Vector3 originalPosition; // the original position of this leg to keep the leg fixed to the ground
    bool isMoving; // to tell the oppsite leg if this one is moving or not
    bool moving; // for this leg to check if its moving or not

    void Start() => originalPosition = transform.position; // to fix the leg to the ground when the game first launches

    void Update()
    {
        var distanceToMoveCube = Vector3.Distance(transform.position, moveCube.transform.position);// calculate the distance from the leg to the cube
        if(distanceToMoveCube >= moveDistance && !oppsiteLeg.isMoving || moving) //to check if the distance is far away from the cube or not and if it is move the leg to the cube
        {
            moving = true; // to tell this leg that it didnt get close enough to stop moving
            transform.position = Vector3.Lerp(transform.position, moveCube.transform.position +  upPosition * Vector3.up, Time.deltaTime * legMoveSpeed); // to move the leg to the cube
            originalPosition = transform.position; // to change the original position and fix the leg to the ground when not moving
            isMoving = true; // to tell oppsite legs that this one is moving

            if(distanceToMoveCube < moveStoppingDistance) // to check if the leg moved all the way instead of just getting in range of the move cube
                moving = false; // to tell the leg it stopped moving and can focus on the ground instead of the cube
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, originalPosition + new Vector3(0f, -0.3f, 0f), Time.deltaTime * legMoveSpeed * 3f); // to move the leg down a bit and make it look like walking instead of sliding
            isMoving = false; //to tell the oppiste leg that this leg is not moving
        }
    }
}