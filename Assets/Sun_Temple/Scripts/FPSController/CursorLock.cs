using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sun_Temple
{
    public class CursorLock : MonoBehaviour
    {
        private bool isLocked;

        void Start()
        {
            isLocked = true;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                isLocked = !isLocked;
            }

            if (ShopManager.IsAnyShopOpen)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                return;
            }

            if (isLocked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }
}
