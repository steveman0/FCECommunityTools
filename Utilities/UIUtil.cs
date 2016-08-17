using System;
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
        ///     Associates the machine so that it, and only it, can handle releasing the UI
        /// </summary>
        public static SegmentEntity TargetMachine;

        /// <summary>
        ///     Vector for recording default window position for scaled UI
        /// </summary>
        private static Vector3 StartPos;

        /// <summary>
        ///     Used to prevent rescaling the UI after it's already been done
        /// </summary>
        private static bool firstopen;

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
                UIUtil.TargetMachine = theMachine;
                theWindow.manager = manager;
                UIUtil.UIdelay = 0;
                UIUtil.UILock = true;
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
        public static void DisconnectUI(SegmentEntity theCaller)
        {
            if (UIdelay > 5 && UILock && UIUtil.TargetMachine != null && UIUtil.TargetMachine == theCaller)
            {
                GenericMachinePanelScript panel = GenericMachinePanelScript.instance;
                panel.gameObject.SetActive(false);
                panel.Background_Panel.SetActive(false);
                panel.currentWindow = null;
                panel.targetEntity = null;
                UIManager.RemoveUIRules("Machine");
                DragAndDropManager.instance.CancelDrag();
                DragAndDropManager.instance.DisableDragBackground();
                GenericMachineManager manager2 = typeof(GenericMachinePanelScript).GetField("manager", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(GenericMachinePanelScript.instance) as GenericMachineManager;
                manager2.windows.Remove(eSegmentEntity.Mod);
                if (manager2.windows.ContainsKey(eSegmentEntity.Mod))
                {
                    Debug.LogWarning("DisconnectUI was not able to remove the window entry!");
                    return;
                }
                UIUtil.TargetMachine = null;
                UILock = false;
            }
            else if (UIUtil.TargetMachine != null && UIUtil.TargetMachine == theCaller)
                UIdelay++;
        }

        /// <summary>
        ///     An emergency escape function when looking at the wrong machine (no longer necessary!)
        /// </summary>
        public static void EscapeUI()
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
        }


        /// <summary>
        ///     Helper class for scaling UI window elements
        /// </summary>
        /// <param name="scalingfactorx">Multiplying factor for the window width</param>
        /// <param name="xoffsetoverride">Allows manual overriding of the window content offset if required</param>
        public static void ScaleUIWindow(float scalingfactorx, float xoffsetoverride = 0)
        {
            if (!firstopen)
            {
                float xoffset = 0;
                if (scalingfactorx > 0)
                    xoffset = -140 * scalingfactorx;
                if (xoffsetoverride != 0)
                    xoffset = xoffsetoverride;
                StartPos = GenericMachinePanelScript.instance.gameObject.transform.localPosition;
                GenericMachinePanelScript.instance.gameObject.transform.localScale = new Vector3(scalingfactorx, 1.0f, 1.0f);
                GenericMachinePanelScript.instance.gameObject.transform.localPosition = StartPos + new Vector3(xoffset, 0f, 0f);
                GenericMachinePanelScript.instance.Background_Panel.transform.localScale = new Vector3(scalingfactorx, 1.0f, 1.0f);
                GenericMachinePanelScript.instance.Label_Holder.transform.localScale = new Vector3(1f / scalingfactorx, 1.0f, 1.0f);
                GenericMachinePanelScript.instance.Icon_Holder.transform.localScale = new Vector3(1f / scalingfactorx, 1.0f, 1.0f);
                GenericMachinePanelScript.instance.Content_Holder.transform.localScale = new Vector3(1f / scalingfactorx, 1.0f, 1.0f);
                GenericMachinePanelScript.instance.Content_Icon_Holder.transform.localScale = new Vector3(1f / scalingfactorx, 1.0f, 1.0f);
                GenericMachinePanelScript.instance.Scroll_Bar.transform.localScale = new Vector3(1f / scalingfactorx, 1.0f, 1.0f);
                GenericMachinePanelScript.instance.Source_Holder.transform.localScale = new Vector3(1f / scalingfactorx, 1.0f, 1.0f);
                GenericMachinePanelScript.instance.Generic_Machine_Title_Label.transform.localScale = new Vector3(1f / scalingfactorx, 1.0f, 1.0f);

                firstopen = true;
            }
        }

        /// <summary>
        ///     Used in UI window OnClose method to restore default window settings
        /// </summary>
        public static void RestoreUIScale()
        {
            GenericMachinePanelScript.instance.gameObject.transform.localPosition = StartPos;
            GenericMachinePanelScript.instance.gameObject.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            GenericMachinePanelScript.instance.Background_Panel.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            GenericMachinePanelScript.instance.Label_Holder.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            GenericMachinePanelScript.instance.Icon_Holder.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            GenericMachinePanelScript.instance.Content_Holder.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            GenericMachinePanelScript.instance.Content_Icon_Holder.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            GenericMachinePanelScript.instance.Scroll_Bar.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            GenericMachinePanelScript.instance.Source_Holder.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            GenericMachinePanelScript.instance.Generic_Machine_Title_Label.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            firstopen = false;
        }
    }
}
