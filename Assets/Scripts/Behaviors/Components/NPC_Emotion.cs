using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using UnityEngine.U2D.Animation;

public class NPC_Emotion : NetworkBehaviour
{
    #region Constants

    //"at", "music", "unhappy", "thirst0", "thirst1", "target", "dazed",
    //"laugh", "happy", "think2", "money", "star", "idea", "none", "think0",
    //"pain", "sleep1", "love1", "alert1", "think0", "hurt", "sleep0",
    //"broken", "alert0", "dead", "cheer", "huh", "love0", "thirst1",
    //"notarget", "angry"

    readonly string[] CommonEmotions =
    {
        "happy", "unhappy", "angry", "thirst0"
    };

    #endregion

    #region Properties

    [SerializeField] float _emotionInterval = 40f;

    #endregion

    #region Member Fields

    SpriteResolver _spriteResolver;
    float _emotionTimer = -1f;

    #endregion

    #region SyncVars

    readonly SyncVar<int> _emotion = new();

    #endregion

    #region RPCs

    [ServerRpc(RequireOwnership = false)]
    void RequestEmotion()
    {
        ObserversSetEmotion(_emotion.Value);
    }

    [ServerRpc(RequireOwnership = false, RunLocally = true)]
    void SetEmotion(int emotion)
    {
        _emotion.Value = emotion;
        ObserversSetEmotion(emotion);
    }

    [ObserversRpc]
    void ObserversSetEmotion(int emotion)
    {
        ChangeEmotion(emotion);
    }

    #endregion

    #region Setup

    void OnEnable()
    {
        var emotion = transform.Find("Emotion");
        if (emotion != null) _spriteResolver = emotion.GetComponent<SpriteResolver>();
    }

    void OnDisable()
    {
    }

    #endregion

    #region Update

    void Update()
    {
        if (!base.HasAuthority) return;

        // TODO: Set this based on emotion instead of randomly.
        if (_emotionTimer < 0f ||
            Time.fixedTime - _emotionTimer > _emotionInterval)
        {
            _emotionTimer = Time.fixedTime;
            int emotion = Random.Range(0, CommonEmotions.Length);
            ChangeEmotion(emotion);
            SetEmotion(emotion);
        }
    }

    #endregion

    #region Private Methods

    void ChangeEmotion(int newValue)
    {
        if (_spriteResolver != null)
        {
            _spriteResolver.SetCategoryAndLabel("Emotes", CommonEmotions[newValue]);
            _spriteResolver.ResolveSpriteToSpriteRenderer();
        }
    }

    #endregion

    public override void OnStartClient()
    {
        if (base.IsClientOnlyStarted)
        {
            RequestEmotion();
        }
    }
}
