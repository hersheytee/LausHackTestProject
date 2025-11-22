namespace Loupedeck.TutorialPlugin
{
    using System;
    using System.Runtime.InteropServices;


    // This class implements an example command that counts button presses.

    public class CounterCommand : PluginDynamicCommand
    {
        Boolean hapticsEnabled = false;

        private string oldState = "buttonPress";
        private Timer timer;

        private int vibrateCount = 0;

        // Initializes the command class.
        public CounterCommand()
            : base(displayName: "Toggle Haptics", description: "Counts button presses", groupName: "Commands")
        {
        }

        // This method is called when the user executes the command.
        protected override void RunCommand(String actionParameter)
        {
            hapticsEnabled = !hapticsEnabled;

            if (hapticsEnabled)
            {
                timer = new Timer(callback, "Some state", TimeSpan.FromMilliseconds(20), TimeSpan.FromMilliseconds(20));
            }
            else
            {
                timer.Change(-1, -1);
            }
        }

        private static string GetCursorState()
        {
            CURSORINFO pci;
            pci.cbSize = Marshal.SizeOf(typeof(CURSORINFO));
            GetCursorInfo(out pci);

            return pci.hCursor.ToString();
        }

        [StructLayout(LayoutKind.Sequential)]
        struct POINT
        {
            public Int32 x;
            public Int32 y;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct CURSORINFO
        {
            public Int32 cbSize;        // Specifies the size, in bytes, of the structure. 
            // The caller must set this to Marshal.SizeOf(typeof(CURSORINFO)).
            public Int32 flags;         // Specifies the cursor state. This parameter can be one of the following values:
            //    0             The cursor is hidden.
            //    CURSOR_SHOWING    The cursor is showing.
            public IntPtr hCursor;          // Handle to the cursor. 
            public POINT ptScreenPos;       // A POINT structure that receives the screen coordinates of the cursor. 
        }

        [DllImport("user32.dll")]
        static extern bool GetCursorInfo(out CURSORINFO pci);




        private void callback(object state)
        {
            if (!hapticsEnabled) return;
            string newState = GetCursorState();
            if (newState != oldState && newState == "65567")
            {
                this.Plugin.PluginEvents.RaiseEvent(
                    "buttonPress"            // Event name (must match YAML files)
                );
                vibrateCount = 0;
            }
            else if (newState == "65567")
            {
                vibrateCount += 1;
                if (vibrateCount % 4 == 0)
                {
                    this.Plugin.PluginEvents.RaiseEvent(
                        "dampHaptic"            // Event name (must match YAML files)
                    );
                }
            }
            else if (newState == "65543")
            {
                vibrateCount += 1;
                if (vibrateCount % 100 == 0)
                {
                    this.Plugin.PluginEvents.RaiseEvent(
                        "loadingHaptic"            // Event name (must match YAML files)
                    );
                }
            }
            oldState = newState;
        }

        protected override Boolean OnLoad()
        {
            this.Plugin.PluginEvents.AddEvent(
                "buttonPress",           // Event name (must match YAML files)
                "Trigger Haptic",           // Display name
                "Plays a haptic"         // Description
            );

            this.Plugin.PluginEvents.AddEvent(
                "dampHaptic",           // Event name (must match YAML files)
                "Trigger Haptic",           // Display name
                "Plays a haptic"         // Description
            );

            this.Plugin.PluginEvents.AddEvent(
                "loadingHaptic",           // Event name (must match YAML files)
                "Trigger Haptic",           // Display name
                "Plays a haptic"         // Description
            );

            return true;
        }

        protected override Boolean OnUnload()
        {
            timer.Change(-1, -1);
            return true;
        }
    }
}
