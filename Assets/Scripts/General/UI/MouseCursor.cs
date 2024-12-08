using UnityEngine;

namespace Shadowfinder.UI
{
    public class MouseCursor : MonoBehaviour
    {
        public Texture2D cursorUp;
        public Texture2D cursorDown;
        public CursorMode cursorMode = CursorMode.Auto;
        public Vector2 hotspot = Vector2.zero;
        public bool autoCenterHotSpot = false;

        void OnEnable()
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            Vector2 hs;
            if (autoCenterHotSpot)
            {
                hotspot = new Vector2(cursorUp.width * 0.5f, cursorUp.height * 0.5f);
                hs = hotspot;
            }
            else
            {
                hs = hotspot;
            }

            Cursor.SetCursor(cursorUp, hs, cursorMode);
        }

        void OnDisable()
        {
            Cursor.SetCursor(null, Vector2.zero, cursorMode);
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
                Cursor.SetCursor(cursorDown, hotspot, cursorMode);
            else if (Input.GetMouseButtonUp(0))
                Cursor.SetCursor(cursorUp, hotspot, cursorMode);
        }
    }
}
