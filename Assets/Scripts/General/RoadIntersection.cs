using FishNet.Object;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RoadIntersection : NetworkBehaviour
{
    [SerializeField] public StoplightColor State = 0;

    [SerializeField] public GameObject Right;
    [SerializeField] public GameObject Left;
    [SerializeField] public GameObject Ahead;

    [SerializeField] GameObject _stoplight;
    [SerializeField] float _lightDuration = 30f;
    [SerializeField] float _yellowLightDuration = 3f;

    float _timer = 0f;

    TilemapRenderer _redLightRenderer;
    TilemapRenderer _yellowLightRenderer;
    TilemapRenderer _greenLightRenderer;

    public void Update()
    {
        _timer += Time.deltaTime;

        switch (State)
        {
            case StoplightColor.Red:
                _redLightRenderer.enabled = true;
                _yellowLightRenderer.enabled = false;
                _greenLightRenderer.enabled = false;

                if (_timer >= _lightDuration)
                {
                    _timer = 0f;
                    State = StoplightColor.Green;
                }
                break;

            case StoplightColor.Yellow:
                _redLightRenderer.enabled = false;
                _yellowLightRenderer.enabled = true;
                _greenLightRenderer.enabled = false;

                if (_timer >= _yellowLightDuration)
                {
                    _timer = 0f;
                    State = StoplightColor.Red;
                }
                break;

            case StoplightColor.Green:
                _redLightRenderer.enabled = false;
                _yellowLightRenderer.enabled = false;
                _greenLightRenderer.enabled = true;

                if (_timer >= _lightDuration)
                {
                    _timer = 0f;
                    State = StoplightColor.Yellow;
                }
                break;
        }
    }

    private void OnEnable()
    {
        var lightRenderers = _stoplight.GetComponentsInChildren<TilemapRenderer>();

        if (lightRenderers.Length != 3)
        {
            Debug.LogError("Expected 3 stoplight lights, found " + lightRenderers.Length);
            return;
        }

        if (lightRenderers[0].gameObject.name == "Red_Tilemap")
            _redLightRenderer = lightRenderers[0];
        if (lightRenderers[1].gameObject.name == "Yellow_Tilemap")
            _yellowLightRenderer = lightRenderers[1];
        if (lightRenderers[2].gameObject.name == "Green_Tilemap")
            _greenLightRenderer = lightRenderers[2];
    }
}
