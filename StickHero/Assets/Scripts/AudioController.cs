using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioController : MonoBehaviour {

   


    #region Fields

    private const float MIN_VOLUME_LVL = 0f;
    private const float MAX_VOLUME_LVL = 0.05f;


    [SerializeField]
    private AudioClip BGSound;
    [SerializeField]
    private AudioClip deathSound;
    [SerializeField]
    private AudioClip SuccessSound;


    private  AudioSource audSrc;

    virtual 
    internal static AudioController audioController = null;
    #endregion


    #region Unity lifecycle
    void Start ()
    {       
        if (audioController == null)
            audioController = this;
        else if (audioController == this)
            Destroy(this.gameObject);

        audSrc = GetComponent<AudioSource>();
        audSrc.volume = MAX_VOLUME_LVL;
    }
    #endregion
    

    #region public mehods
    public void PlaySound(string clipName)
    {
        switch (clipName)
        {

            case "BGSound":
                audSrc.PlayOneShot(BGSound);
                break;
            case "kill":
                audSrc.PlayOneShot(deathSound);
                break;
            case "success":
                audSrc.PlayOneShot(SuccessSound);
                break;
        }
    }


    public void SoundButton()
    {
        if (audSrc.volume == MAX_VOLUME_LVL)
            audSrc.volume = MIN_VOLUME_LVL;
        else if (audSrc.volume == MIN_VOLUME_LVL)
            audSrc.volume = MAX_VOLUME_LVL;

    }

    #endregion
}
