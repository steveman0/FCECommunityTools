﻿using System;
using System.Reflection;
using UnityEngine;

namespace FortressCraft.Community.Utilities
{
    /// <summary>
    ///     Simple cross-mod compatible UI support.  Original code by BinaryAlgorithm, updated by steveman0
    /// </summary>
    public class UIUtil
    {
        /// <summary>
        ///     Timer to delay dissociation of UI panel to overcome race condition
        /// </summary>
        public static int UIdelay;
        /// <summary>
        ///     Lock to prevent running the dissociation when it isn't needed
        /// </summary>
        public static bool UILock;

        /// <summary>
        ///     Call this in GetPopupText to handle your UI Window
        /// </summary>
        /// <param name="theMachine">Pass the current machine</param>
        /// <param name="theWindow">The mod window inherited from BaseMachineWindow</param>
        /// <returns></returns>
        public static bool HandleThisMachineWindow(SegmentEntity theMachine, BaseMachineWindow theWindow)
        {
            try
            {
                //GenericMachineManager manager = GenericMachinePanelScript.instance.manager; // not yet
                GenericMachineManager manager = typeof(GenericMachinePanelScript).GetField("manager", BindingFlags.NonPublic |
                    BindingFlags.Instance).GetValue(GenericMachinePanelScript.instance) as GenericMachineManager;

                // this will replace with the current machine's window, which is OK as long as each Mod uses this technique
                manager.windows[eSegmentEntity.Mod] = theWindow;
                theWindow.manager = manager;
            }
            catch (Exception ex)
            {
                //this.error = "Window Registration failed : " + ex.Message;
                UnityEngine.Debug.LogError("Window Registration failed : " + ex.Message + " : " + ex.StackTrace);
            }
            GenericMachinePanelScript panel = GenericMachinePanelScript.instance;

            try
            {
                // player looking at this machine
                if (WorldScript.instance.localPlayerInstance.mPlayerBlockPicker.selectedEntity == theMachine)
                {
                    if (panel.Background_Panel.activeSelf == true) // window is open
                    {
                        panel.gameObject.SetActive(true); // undoes the default handler hiding the window
                    }

                    // player interacts with machine
                    if (Input.GetButtonDown("Interact")) // "E" by default
                    {
                        // UIManager.UpdateGenericMachineWindow() -> GenericMachinePanelScript.TryShow()
                        // default handler will try and fail as intended because our window is not in its dictionary, this is fine
                        // similarly, the Hide() should not occur because the selectedEntity is this machine (panel.targetEntity)
                        //Debug.Log("Interacted");
                        if (panel.Background_Panel.activeSelf == true) // window is not already opened
                        {
                            // Do nothing
                        }
                        else // window IS already opened, we pressed to interact again (meaning to close it)
                        {
                            Hide(panel); // hide window, since we are not focused on this machine anymore
                            DragAndDropManager.instance.CancelDrag();
                            DragAndDropManager.instance.DisableDragBackground();
                        }
                    }
                    else if (Input.GetKeyDown(KeyCode.Escape)) // escape should close window also
                    {
                        if (panel.isActiveAndEnabled)
                            UIManager.instance.UnpauseGame();
                        Hide(panel); // hide window
                        DragAndDropManager.instance.CancelDrag();
                        DragAndDropManager.instance.DisableDragBackground();
                    }
                }
                else // we are not the selected machine, or no machine is selected; but not due to user input (probably)
                {
                    if (panel.targetEntity == theMachine) // this machine WAS focused with window open a moment ago, so it should handle closing its own window
                    {
                        Hide(panel); // hide window, since we are not focused on this machine anymore
                        DragAndDropManager.instance.CancelDrag();
                        DragAndDropManager.instance.DisableDragBackground();
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }

            return (panel.targetEntity == theMachine); // true if machine window is currently open, false otherwise (such as if Hide() called)
        }

        /// <summary>
        ///     Internal sub function for hiding the panel
        /// </summary>
        /// <param name="panel">The working panel</param>
        public static void Hide(GenericMachinePanelScript panel)
        {
            UIManager.RemoveUIRules("Machine");
            panel.currentWindow.OnClose(panel.targetEntity);
            panel.Scroll_Bar.GetComponent<UIScrollBar>().scrollValue = 0f;
            panel.targetEntity = null;
            panel.currentWindow = null;

            GenericMachineManager manager = typeof(GenericMachinePanelScript).GetField("manager", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(panel) as GenericMachineManager;
            manager.ClearWindow();

            panel.gameObject.SetActive(false);
            panel.Background_Panel.SetActive(false);
        }

        /// <summary>
        ///     Insert in machine UnityUpdate to disconnect the UI when finished.
        /// </summary>
        public static void DisconnectUI()
        {
            if (UIdelay > 30 && UILock)
            {
                GenericMachinePanelScript panel = GenericMachinePanelScript.instance;
                panel.gameObject.SetActive(false);
                panel.Background_Panel.SetActive(false);
                panel.currentWindow = null;
                panel.targetEntity = null;
                GenericMachineManager manager2 = typeof(GenericMachinePanelScript).GetField("manager", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(GenericMachinePanelScript.instance) as GenericMachineManager;
                manager2.windows.Remove(eSegmentEntity.Mod);
                UIManager.RemoveUIRules("Machine");
                DragAndDropManager.instance.CancelDrag();
                DragAndDropManager.instance.DisableDragBackground();
                UILock = false;
            }
            UIdelay++;
        }
    }
}