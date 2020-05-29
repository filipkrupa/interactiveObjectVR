using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowBubble : InteractableObject
{
    [SerializeField]
    [Tooltip("An array of gameObjects to show when player looks at somebody")]
    private GameObject[] bubbleObjects;

    private bool timerOn = false;
    private float timer = 0.0f;
    private float stayTime = 5.0f;

    public override void OnLook()
    {
        Debug.Log("showing bubbles");

        //hightlight the character when the player looks at him/her
        Highlight();

        //show the player options to choose from
        foreach (GameObject bubble in bubbleObjects)
        {
            activeRenderer(bubble);
        }

        timerOn = false;
    }

    public override void OnLookAway()
    {
        timer = 0.0f;
        timerOn = true;

        Debug.Log("hidding bubbles");
        removeHighlight();
    }

    public override void OnClick()
    {
        foreach (GameObject bubble in bubbleObjects)
        {
            deactivateRenderer(bubble);
        }

    }

    public override void MyStart()
    {
        foreach (GameObject bubble in bubbleObjects)
        {
            deactivateRenderer(bubble);
        }
    }



    public override void MyUpdate()
    {
        if (timerOn)
        {
            timer += Time.deltaTime;
            if (timer > stayTime)
            {
                foreach (GameObject bubble in bubbleObjects)
                {
                    deactivateRenderer(bubble);
                }
            }
        }
    }

    private void activeRenderer(GameObject o)
    {
        MeshRenderer[] renderers = o.GetComponentsInChildren<MeshRenderer>();
        foreach (var renderer in renderers)
        {
            renderer.enabled = true;
        }

        MeshRenderer[] renderers2 = o.GetComponents<MeshRenderer>();
        foreach (var renderer in renderers)
        {
            renderer.enabled = true;
        }
    }

    private void deactivateRenderer(GameObject o)
    {
        MeshRenderer[] renderers = o.GetComponentsInChildren<MeshRenderer>();
        foreach (var renderer in renderers)
        {
            renderer.enabled = false;
        }

        MeshRenderer[] renderers2 = o.GetComponents<MeshRenderer>();
        foreach (var renderer in renderers)
        {
            renderer.enabled = false;
        }
    }
}
