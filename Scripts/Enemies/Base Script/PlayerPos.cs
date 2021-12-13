 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPos : MonoBehaviour
{
    public static bool FindPlayerPos(float inRange, Transform playerPos, float inView, GameObject self, bool hasSeenPlayer)
    {
        float distDif = Vector3.Distance(playerPos.position, self.transform.position);
        if (distDif <= inRange && PlayerIsInSight(inView, playerPos, self) && !hasSeenPlayer)
        {
            hasSeenPlayer = true;            
            return hasSeenPlayer;
            
        }

        if (distDif <= inRange && hasSeenPlayer)
        {
            self.transform.LookAt(playerPos.position);            
            return true;
        }        
        return false;
    }

   
    public static bool PlayerIsInSight(float inView, Transform playerPos, GameObject self)
    {
        Vector3 targetDir = playerPos.position - self.transform.position;
        targetDir.y = 0f;
        Vector3 forward = self.transform.forward;
        float angle = Vector3.SignedAngle(targetDir, forward, Vector3.up);
        float selfHeight = self.GetComponent<Collider>().bounds.size.y;

        if (angle <= inView && angle >= -inView && playerPos.position.y <= self.transform.position.y + selfHeight/2 + selfHeight && playerPos.position.y >= (self.transform.position.y - selfHeight) - selfHeight / 2)
        {
            return true;
        }
        return false;
    }
}
