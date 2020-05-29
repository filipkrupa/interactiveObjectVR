using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TalkBubble :  InteractableObject
{
  
    [SerializeField]
    Animator anim;



    public override void OnClick()
    {
        anim.SetTrigger("talk_animation");
        AudioSource Audio = GetComponent<AudioSource>();
        if (Audio != null)
            Audio.Play();
        else
            GetComponent<MultipleTalkScript>().getRandomSource().Play();
    }

    public override void OnLook()
    {
        Highlight();
    }

    public override void OnLookAway()
    {
        removeHighlight();
    }
}


