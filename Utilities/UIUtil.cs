using System;
using System.Reflection;
using UnityEngine;

namespace FortressCraft.Community.Utilities
{
    /// <summary>
    ///     Simple cross-mod compatible UI support.  Original code by BinaryAlgorithm, updated by steveman0, with added features provided by Shadow
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
        ///     Associates the TargetWindow with the UI
        /// </summary>
        public static BaseMachineWindow TargetWindow;

        /// <summary>
        ///     Vector for recording default window position for scaled UI
        /// </summary>
        private static Vector3 StartPos;
        private static Vector3 BackgroundStartPos;


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
                UIUtil.TargetWindow = theWindow;

                bool flag1 = SetupMachineWindow(theMachine);
                if (!flag1)
                {
                    return false;
                }

                bool flag2 = HandleWindowView(theMachine);
                if (!flag2)
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("HandleThisMachineWindow: failed : " + ex.Message + " : " + ex.StackTrace);
            }

            return true;
        }

        /// <summary>
        ///     Call this in HandleThisMachineWindow to handle your UI Window
        /// </summary>
        /// <param name="theMachine">Pass the current machine</param>
        /// <param name="theWindow">The mod window inherited from BaseMachineWindow</param>
        /// <returns></returns>
        public static bool SetupMachineWindow(SegmentEntity theMachine)
        {
            //ModLogging.LogPlain(debugLocal, getPrefix(), "SetupMachineWindow: Start");

            bool flag = false;

            try
            {
                UIUtil.UIdelay = 0;
                UIUtil.UILock = false;

                //GenericMachineManager manager = GenericMachinePanelScript.instance.manager; // not yet
                GenericMachineManager manager = typeof(GenericMachinePanelScript).GetField("manager", BindingFlags.NonPublic |
                    BindingFlags.Instance).GetValue(GenericMachinePanelScript.instance) as GenericMachineManager;

                // this will replace with the current machine's window, which is OK as long as each Mod uses this technique
                manager.windows[eSegmentEntity.Mod] = UIUtil.TargetWindow;

                UIUtil.TargetMachine = theMachine;
                UIUtil.TargetWindow.manager = manager;

                UIUtil.UIdelay = 0;
                UIUtil.UILock = true;

                flag = true;
            }
            catch (Exception ex)
            {
                //this.error = "Window Registration failed : " + ex.Message;
                UnityEngine.Debug.LogError("SetupMachineWindow: failed : " + ex.Message + " : " + ex.StackTrace);
            }

            //ModLogging.LogPlain(debugLocal, getPrefix(), "SetupMachineWindow: End");

            return flag;
        }

        /// <summary>
        ///     Call this in HandleThisMachineWindow to handle your UI Window Viewing
        /// </summary>
        /// <param name="theMachine">Pass the current machine</param>
        /// <returns></returns>
        public static bool HandleWindowView(SegmentEntity theMachine)
        {
            bool flag = false;
            try
            {
                GenericMachinePanelScript panel = GenericMachinePanelScript.instance;

                // player looking at this machine
                if (WorldScript.instance.localPlayerInstance.mPlayerBlockPicker.selectedEntity == theMachine)
                {
                    //ModLogging.LogPlain(debugLocal, getPrefix(), "HandleWindowView: We are theMachine");

                    if (panel.Background_Panel.activeSelf) // window is open
                    {
                        //ModLogging.LogPlain(debugLocal, getPrefix(), "HandleWindowView: Panel SetActive");
                        panel.gameObject.SetActive(true); // undoes the default handler hiding the window
                    }

                    //ModLogging.LogPlain(debugLocal, getPrefix(), "HandleWindowView: TestButton");

                    // player interacts with machine
                    if (Input.GetButtonDown("Interact")) // "E" by default
                    {
                        //ModLogging.LogPlain(debugLocal, getPrefix(), "HandleWindowView: Interact Button Pressed");

                        // UIManager.UpdateGenericMachineWindow() -> GenericMachinePanelScript.TryShow()
                        // default handler will try and fail as intended because our window is not in its dictionary, this is fine
                        // similarly, the Hide() should not occur because the selectedEntity is this machine (panel.targetEntity)
                        //Debug.Log("Interacted");

                        if (panel.Background_Panel.activeSelf) // window is not already opened
                        {
                            //ModLogging.LogPlain(debugLocal, getPrefix(), "HandleWindowView: Panel Active - Do Nothing");
                            // Do nothing - GenericMachinePanelScript will handle
                        }
                        else // window IS already opened, we pressed to interact again (meaning to close it)
                        {
                            //ModLogging.LogPlain(debugLocal, getPrefix(), "HandleWindowView: Hide Panel");
                            Hide(panel); // hide window, since we are not focused on this machine anymore
                        }
                    }
                    else if (Input.GetKeyDown(KeyCode.Escape)) // escape should close window also
                    {
                        //ModLogging.LogPlain(debugLocal, getPrefix(), "HandleWindowView:Escape Button Pressed");

                        if (panel.isActiveAndEnabled)
                            UIManager.instance.UnpauseGame();
                        Hide(panel); // hide window
                    }
                }
                else // we are not the selected machine, or no machine is selected; but not due to user input (probably)
                {
                    if (panel.targetEntity == theMachine) // this machine WAS focused with window open a moment ago, so it should handle closing its own window
                    {
                        Hide(panel); // hide window, since we are not focused on this machine anymore
                    }
                }
                flag = (panel.targetEntity == theMachine); // true if machine window is currently open, false otherwise (such as if Hide() called)
            }
            catch (Exception ex)
            {
                Debug.LogError("HandleWindowView: failed : " + ex.Message + " : " + ex.StackTrace);
                flag = false;
            }
            return flag;
        }

        ///// <summary>
        /////     Call this in GetPopupText to handle your UI Window
        ///// </summary>
        ///// <param name="theMachine">Pass the current machine</param>
        ///// <param name="theWindow">The mod window inherited from BaseMachineWindow</param>
        ///// <returns></returns>
        //public static bool HandleThisMachineWindow(SegmentEntity theMachine, BaseMachineWindow theWindow)
        //{
        //    try
        //    {
        //        //GenericMachineManager manager = GenericMachinePanelScript.instance.manager; // not yet
        //        GenericMachineManager manager = typeof(GenericMachinePanelScript).GetField("manager", BindingFlags.NonPublic |
        //            BindingFlags.Instance).GetValue(GenericMachinePanelScript.instance) as GenericMachineManager;

        //        // this will replace with the current machine's window, which is OK as long as each Mod uses this technique
        //        manager.windows[eSegmentEntity.Mod] = theWindow;
        //        UIUtil.TargetMachine = theMachine;
        //        theWindow.manager = manager;
        //        UIUtil.UIdelay = 0;
        //        UIUtil.UILock = true;
        //    }
        //    catch (Exception ex)
        //    {
        //        //this.error = "Window Registration failed : " + ex.Message;
        //        UnityEngine.Debug.LogError("Window Registration failed : " + ex.Message + " : " + ex.StackTrace);
        //    }
        //    GenericMachinePanelScript panel = GenericMachinePanelScript.instance;

        //    try
        //    {
        //        // player looking at this machine
        //        if (WorldScript.instance.localPlayerInstance.mPlayerBlockPicker.selectedEntity == theMachine)
        //        {
        //            if (panel.Background_Panel.activeSelf == true) // window is open
        //            {
        //                panel.gameObject.SetActive(true); // undoes the default handler hiding the window
        //            }

        //            // player interacts with machine
        //            if (Input.GetButtonDown("Interact")) // "E" by default
        //            {
        //                // UIManager.UpdateGenericMachineWindow() -> GenericMachinePanelScript.TryShow()
        //                // default handler will try and fail as intended because our window is not in its dictionary, this is fine
        //                // similarly, the Hide() should not occur because the selectedEntity is this machine (panel.targetEntity)
        //                //Debug.Log("Interacted");
        //                if (panel.Background_Panel.activeSelf == true) // window is not already opened
        //                {
        //                    // Do nothing
        //                }
        //                else // window IS already opened, we pressed to interact again (meaning to close it)
        //                {
        //                    Hide(panel); // hide window, since we are not focused on this machine anymore
        //                    DragAndDropManager.instance.CancelDrag();
        //                    DragAndDropManager.instance.DisableDragBackground();
        //                }
        //            }
        //            else if (Input.GetKeyDown(KeyCode.Escape)) // escape should close window also
        //            {
        //                if (panel.isActiveAndEnabled)
        //                    UIManager.instance.UnpauseGame();
        //                Hide(panel); // hide window
        //                DragAndDropManager.instance.CancelDrag();
        //                DragAndDropManager.instance.DisableDragBackground();
        //            }
        //        }
        //        else // we are not the selected machine, or no machine is selected; but not due to user input (probably)
        //        {
        //            if (panel.targetEntity == theMachine) // this machine WAS focused with window open a moment ago, so it should handle closing its own window
        //            {
        //                Hide(panel); // hide window, since we are not focused on this machine anymore
        //                DragAndDropManager.instance.CancelDrag();
        //                DragAndDropManager.instance.DisableDragBackground();
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return false;
        //    }

        //    return (panel.targetEntity == theMachine); // true if machine window is currently open, false otherwise (such as if Hide() called)
        //}

        ///// <summary>
        /////     Internal sub function for hiding the panel
        ///// </summary>
        ///// <param name="panel">The working panel</param>
        //public static void Hide(GenericMachinePanelScript panel)
        //{
        //    UIManager.RemoveUIRules("Machine");
        //    panel.currentWindow.OnClose(panel.targetEntity);
        //    panel.Scroll_Bar.GetComponent<UIScrollBar>().scrollValue = 0f;
        //    panel.targetEntity = null;
        //    panel.currentWindow = null;

        //    GenericMachineManager manager = typeof(GenericMachinePanelScript).GetField("manager", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(panel) as GenericMachineManager;
        //    manager.ClearWindow();

        //    panel.gameObject.SetActive(false);
        //    panel.Background_Panel.SetActive(false);
        //}

        /// <summary>
        ///     Internal sub function for hiding the panel
        /// </summary>
        /// <param name="panel">The working panel</param>
        public static void Hide(GenericMachinePanelScript panel)
        {
            try
            {
                UIManager.RemoveUIRules("Machine");

                if (panel == null)
                {
                    //Debug.LogError("Hide: panel=null");
                    return;
                }
                else
                {
                    if (panel.targetEntity == null)
                    {
                        //Debug.LogError("Hide: panel.targetEntity=null");
                    }
                    else
                    {
                        panel.currentWindow.OnClose(panel.targetEntity);
                    }

                    panel.Scroll_Bar.GetComponent<UIScrollBar>().scrollValue = 0f;

                    panel.targetEntity = null;
                    panel.currentWindow = null;

                    panel.gameObject.SetActive(false);
                    panel.Background_Panel.SetActive(false);

                    GenericMachineManager manager = typeof(GenericMachinePanelScript).GetField("manager",
                        BindingFlags.NonPublic | BindingFlags.Instance).GetValue(panel) as GenericMachineManager;
                    manager.ClearWindow();

                }

                DragAndDropManager.instance.CancelDrag();
                DragAndDropManager.instance.DisableDragBackground();

            }
            catch (Exception ex)
            {
                Debug.LogError("Hide: " + ex.Message + " : " + ex.StackTrace);
            }
        }

        /// <summary>
        ///     Insert in machine UnityUpdate to disconnect the UI when finished.
        /// </summary>
        public static void DisconnectUI(SegmentEntity theCaller)
        {
            //ModLogging.LogPlain(debugLocal, getPrefix(), "DisconnectUI: Start");

            try
            {
                if (UIUtil.UIdelay > 5 && UIUtil.UILock && UIUtil.TargetMachine != null && UIUtil.TargetMachine == theCaller)
                {
                    CleanupUI("DisconnectUI", true);
                }
                else if (UIUtil.TargetMachine != null && UIUtil.TargetMachine == theCaller)
                {
                    UIUtil.UIdelay++;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("DisconnectUI: " + ex.Message + " : " + ex.StackTrace);
            }
        }

        ///// <summary>
        /////     Insert in machine UnityUpdate to disconnect the UI when finished.
        ///// </summary>
        //public static void DisconnectUI(SegmentEntity theCaller)
        //{
        //    if (UIdelay > 5 && UILock && UIUtil.TargetMachine != null && UIUtil.TargetMachine == theCaller)
        //    {
        //        GenericMachinePanelScript panel = GenericMachinePanelScript.instance;
        //        panel.gameObject.SetActive(false);
        //        panel.Background_Panel.SetActive(false);
        //        panel.currentWindow = null;
        //        panel.targetEntity = null;
        //        UIManager.RemoveUIRules("Machine");
        //        DragAndDropManager.instance.CancelDrag();
        //        DragAndDropManager.instance.DisableDragBackground();
        //        GenericMachineManager manager2 = typeof(GenericMachinePanelScript).GetField("manager", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(GenericMachinePanelScript.instance) as GenericMachineManager;
        //        manager2.windows.Remove(eSegmentEntity.Mod);
        //        if (manager2.windows.ContainsKey(eSegmentEntity.Mod))
        //        {
        //            Debug.LogWarning("DisconnectUI was not able to remove the window entry!");
        //            return;
        //        }
        //        UIUtil.TargetMachine = null;
        //        UILock = false;
        //    }
        //    else if (UIUtil.TargetMachine != null && UIUtil.TargetMachine == theCaller)
        //        UIdelay++;
        //}

        /// <summary>
        ///     An emergency escape function when looking at the wrong machine (no longer necessary!)
        /// </summary>
        public static void EscapeUI()
        {
            try
            {
                CleanupUI("EscapeUI", true);
            }
            catch (Exception ex)
            {
                Debug.LogError("EscapeUI: " + ex.Message + " : " + ex.StackTrace);
            }
        }

        /// <summary>
        ///     Closes out and removes references to .Mod UI entiries
        /// </summary>
        /// <param name="name"></param>
        /// <param name="resetWindow"></param>
        public static void CleanupUI(string name, bool resetWindow)
        {
            try
            {
                GenericMachinePanelScript panel = GenericMachinePanelScript.instance;

                UIManager.RemoveUIRules("Machine");

                if (panel == null)
                {
                    Debug.LogError("CleanupUI: panel=null");
                    return;
                }
                else
                {
                    UIUtil.TargetMachine = null;

                    if (resetWindow)
                    {
                        UIUtil.TargetWindow = null;
                    }

                    panel.gameObject.SetActive(false);
                    panel.Background_Panel.SetActive(false);

                    panel.currentWindow = null;
                    panel.targetEntity = null;

                    DragAndDropManager.instance.CancelDrag();
                    DragAndDropManager.instance.DisableDragBackground();

                    UIUtil.UILock = false;

                    GenericMachineManager manager = typeof(GenericMachinePanelScript).GetField("manager",
                        BindingFlags.NonPublic | BindingFlags.Instance).GetValue(panel) as GenericMachineManager;

                    manager.windows.Remove(eSegmentEntity.Mod);

                    if (manager.windows.ContainsKey(eSegmentEntity.Mod))
                    {
                        Debug.LogWarning(name + ": was not able to remove the window entry!");
                        return;
                    }
                }

            }
            catch (Exception ex)
            {
                Debug.LogError("CleanupUI: " + ex.Message + " : " + ex.StackTrace);
            }
        }

        ///// <summary>
        /////     An emergency escape function when looking at the wrong machine (no longer necessary!)
        ///// </summary>
        //public static void EscapeUI()
        //{
        //    GenericMachinePanelScript panel = GenericMachinePanelScript.instance;
        //    panel.gameObject.SetActive(false);
        //    panel.Background_Panel.SetActive(false);
        //    panel.currentWindow = null;
        //    panel.targetEntity = null;
        //    GenericMachineManager manager2 = typeof(GenericMachinePanelScript).GetField("manager", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(GenericMachinePanelScript.instance) as GenericMachineManager;
        //    manager2.windows.Remove(eSegmentEntity.Mod);
        //    UIManager.RemoveUIRules("Machine");
        //    DragAndDropManager.instance.CancelDrag();
        //    DragAndDropManager.instance.DisableDragBackground();
        //}


        ///// <summary>
        /////     Helper class for scaling UI window elements
        ///// </summary>
        ///// <param name="scalingfactorx">Multiplying factor for the window width</param>
        ///// <param name="xoffsetoverride">Allows manual overriding of the window content offset if required</param>
        //public static void ScaleUIWindow(float scalingfactorx, float xoffsetoverride = 0)
        //{
        //    if (!firstopen)
        //    {
        //        float xoffset = 0;
        //        if (scalingfactorx > 0)
        //            xoffset = -140 * scalingfactorx;
        //        if (xoffsetoverride != 0)
        //            xoffset = xoffsetoverride;
        //        StartPos = GenericMachinePanelScript.instance.gameObject.transform.localPosition;
        //        GenericMachinePanelScript.instance.gameObject.transform.localScale = new Vector3(scalingfactorx, 1.0f, 1.0f);
        //        GenericMachinePanelScript.instance.gameObject.transform.localPosition = StartPos + new Vector3(xoffset, 0f, 0f);
        //        GenericMachinePanelScript.instance.Background_Panel.transform.localScale = new Vector3(scalingfactorx, 1.0f, 1.0f);
        //        GenericMachinePanelScript.instance.Label_Holder.transform.localScale = new Vector3(1f / scalingfactorx, 1.0f, 1.0f);
        //        GenericMachinePanelScript.instance.Icon_Holder.transform.localScale = new Vector3(1f / scalingfactorx, 1.0f, 1.0f);
        //        GenericMachinePanelScript.instance.Content_Holder.transform.localScale = new Vector3(1f / scalingfactorx, 1.0f, 1.0f);
        //        GenericMachinePanelScript.instance.Content_Icon_Holder.transform.localScale = new Vector3(1f / scalingfactorx, 1.0f, 1.0f);
        //        GenericMachinePanelScript.instance.Scroll_Bar.transform.localScale = new Vector3(1f / scalingfactorx, 1.0f, 1.0f);
        //        GenericMachinePanelScript.instance.Source_Holder.transform.localScale = new Vector3(1f / scalingfactorx, 1.0f, 1.0f);
        //        GenericMachinePanelScript.instance.Generic_Machine_Title_Label.transform.localScale = new Vector3(1f / scalingfactorx, 1.0f, 1.0f);

        //        firstopen = true;
        //    }
        //}

        ///// <summary>
        /////     Used in UI window OnClose method to restore default window settings
        ///// </summary>
        //public static void RestoreUIScale()
        //{
        //    GenericMachinePanelScript.instance.gameObject.transform.localPosition = StartPos;
        //    GenericMachinePanelScript.instance.gameObject.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        //    GenericMachinePanelScript.instance.Background_Panel.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        //    GenericMachinePanelScript.instance.Label_Holder.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        //    GenericMachinePanelScript.instance.Icon_Holder.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        //    GenericMachinePanelScript.instance.Content_Holder.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        //    GenericMachinePanelScript.instance.Content_Icon_Holder.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        //    GenericMachinePanelScript.instance.Scroll_Bar.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        //    GenericMachinePanelScript.instance.Source_Holder.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        //    GenericMachinePanelScript.instance.Generic_Machine_Title_Label.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        //    firstopen = false;
        //}

        /// <summary>
        ///     Helper class for scaling UI window elements
        /// </summary>
        /// <param name="scalingfactorx">Multiplying factor for the window width</param>
        /// <param name="xoffsetoverride">Allows manual overriding of the window content offset if required</param>
        public static void ScaleUIWindow(float scalingfactorx, float xoffsetoverride = 0)
        {
            try
            {
                GenericMachinePanelScript panel = GenericMachinePanelScript.instance;
                if (!firstopen)
                {
                    firstopen = true;

                    if (scalingfactorx < 0.5f)
                    {
                        scalingfactorx = 0.5f;
                    }
                    else if (scalingfactorx > 5.0f)
                    {
                        scalingfactorx = 5.0f;
                    }

                    float xoffset = 0f;

                    if (Math.Abs(scalingfactorx - 1.0f) > Single.Epsilon) // 1f
                    {
                        xoffset = (-140f * scalingfactorx);
                    }

                    if (Math.Abs(xoffsetoverride) > Single.Epsilon) // 0f
                    {
                        xoffset = xoffsetoverride;
                    }

                    UIUtil.StartPos = panel.gameObject.transform.localPosition;
                    UIUtil.BackgroundStartPos = panel.Background_Panel.transform.localPosition; // Not Used

                    panel.gameObject.transform.localScale = new Vector3(scalingfactorx, 1.0f, 1.0f);
                    panel.Background_Panel.transform.localScale = new Vector3(scalingfactorx, 1.0f, 1.0f);

                    panel.gameObject.transform.localPosition = UIUtil.StartPos + new Vector3(xoffset, 0f, 0f);
                    // my testing only: adding this causes the window frame to be ofset up/left of background
                    //panel.Background_Panel.transform.localPosition = UIUtil.BackgroundStartPos + new Vector3(xoffset, 0f, 0f); //added
                    //SetPosition(panel.gameObject, UIUtil.StartPos + new Vector3(xoffset, 0f, 0f));
                    //SetPosition(panel.Background_Panel, UIUtil.BackgroundStartPos + new Vector3(xoffset, 0f, 0f));

                    if (Math.Abs(scalingfactorx - 1f) < Single.Epsilon) // 1f
                    {
                        return;
                    }

                    float adjFactor = (1f / scalingfactorx); // added: lets divide 1 time

                    panel.Label_Holder.transform.localScale = new Vector3(adjFactor, 1.0f, 1.0f);
                    panel.Icon_Holder.transform.localScale = new Vector3(adjFactor, 1.0f, 1.0f);
                    panel.Content_Holder.transform.localScale = new Vector3(adjFactor, 1.0f, 1.0f);
                    panel.Content_Icon_Holder.transform.localScale = new Vector3(adjFactor, 1.0f, 1.0f);
                    panel.Scroll_Bar.transform.localScale = new Vector3(adjFactor, 1.0f, 1.0f);
                    panel.Source_Holder.transform.localScale = new Vector3(adjFactor, 1.0f, 1.0f);

                    //panel.Generic_Machine_Title_Label.transform.localScale = new Vector3(adjFactor, 1.0f, 1.0f); // Leave Off - Fixes Title Sizing
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("ScaleUIWindow: " + ex.Message + " : " + ex.StackTrace);
            }
        }

        /// <summary>
        ///     Used in UI window OnClose method to restore default window settings
        /// </summary>
        public static void RestoreUIScale()
        {
            try
            {
                GenericMachinePanelScript panel = GenericMachinePanelScript.instance;

                panel.gameObject.transform.localPosition = UIUtil.StartPos;
                // panel.Background_Panel.transform.localPosition = UIUtil.BackgroundStartPos; // Not Used
                //SetPosition(panel.gameObject, UIUtil.StartPos);
                //SetPosition(panel.Background_Panel, UIUtil.BackgroundStartPos);

                panel.gameObject.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                panel.Background_Panel.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

                panel.Label_Holder.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                panel.Icon_Holder.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                panel.Content_Holder.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                panel.Content_Icon_Holder.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                panel.Scroll_Bar.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                panel.Source_Holder.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

                //panel.Generic_Machine_Title_Label.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f); // Leave Off - Fixes Title Sizing

                firstopen = false;
            }
            catch (Exception ex)
            {
                Debug.LogError("RestoreUIScale: " + ex.Message + " : " + ex.StackTrace);
            }
        }


        public static bool SetButtonLabelKey(string name)
        {
            bool flag = false;
            string key = name + "_label";

            try
            {
                GenericMachineManager manager = typeof(GenericMachinePanelScript).GetField("manager", BindingFlags.NonPublic |
                    BindingFlags.Instance).GetValue(GenericMachinePanelScript.instance) as GenericMachineManager;

                int cnt = manager.mWindowContents.Count;
                int find = -1;
                int offset = 1;

                GameObject gObj = new GameObject();

                //ModLogging.LogError(getPrefix(), "SetButtonLabelKey: cnt : [" + cnt + "]");
                if (cnt > 0)
                {
                    for (int idx = 0; idx < cnt; idx++)
                    {

                        gObj = manager.mWindowContents[idx];
                        //ModLogging.LogError(getPrefix(), "SetButtonLabelKey: cnt : [" + cnt + ":" + idx + ":" + gObj.name + "]");
                        if (gObj.name == name)
                        {
                            //ModLogging.LogError(getPrefix(), "SetButtonLabelKey: found: " + idx);
                            find = idx;
                            //break;
                        }
                    }

                    //ModLogging.LogError(getPrefix(), "SetButtonLabelKey: test " + find + ":" + offset + ":" + cnt + ":" + key);
                    if (find != -1)
                    {
                        if (find + offset < cnt)
                        {
                            gObj.name = key;
                            flag = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("SetButtonLabelKey: failed : [" + key + "]" + ex.Message + " : " + ex.StackTrace);
            }

            return flag;

        }

        /// <summary>
        ///     Updates the label on the button
        /// </summary>
        /// <param name="name">UI Button name</param>
        /// <param name="label">New text label</param>
        public static void UpdateButtonLabel(string name, string label)
        {
            try
            {
                string key = name + "_label";

                //ModLogging.LogError(getPrefix(), "UpdateButtonLabel: key : [" + key + "]");

                GenericMachineManager manager = typeof(GenericMachinePanelScript).GetField("manager", BindingFlags.NonPublic |
                  BindingFlags.Instance).GetValue(GenericMachinePanelScript.instance) as GenericMachineManager;

                int cnt = manager.mWindowContents.Count;

                //ModLogging.LogError(getPrefix(), "UpdateButtonLabel: manager : [" + manager.mWindowContents.Count + ":" + label + ":" + cnt + "]");

                if (cnt > 0)
                {
                    foreach (GameObject current in manager.mWindowContents)
                    {
                        if (current.name == key)
                        {
                            //ModLogging.LogError(getPrefix(), "UpdateButtonLabel: inside[" + current.name + ":" + key + ":" + cnt + "]");
                            current.GetComponent<UILabel>().text = label;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("UpdateButtonLabel: failed : " + ex.Message + " : " + ex.StackTrace);
            }

        }
    }
}
