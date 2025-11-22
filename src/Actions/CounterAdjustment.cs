namespace Loupedeck.TutorialPlugin
{
    using System;
    using System.Runtime.CompilerServices;

    using System.Runtime.InteropServices;

    // This class implements an example adjustment that counts the rotation ticks of a dial.

    public class CounterAdjustment : PluginDynamicAdjustment
    {
        // This variable holds the current value of the counter.
        private Int32 _counter = 0;

        // Initializes the adjustment class.
        // When `hasReset` is set to true, a reset command is automatically created for this adjustment.
        public CounterAdjustment()
            : base(displayName: "Tick Counter", description: "Counts rotation ticks", groupName: "Adjustments", hasReset: true)
        {
        }

        // This method is called when the adjustment is executed.
        protected override void ApplyAdjustment(String actionParameter, Int32 diff)
        {
            this._counter += diff; // Increase or decrease the counter by the number of ticks.
            this.AdjustmentValueChanged(); // Notify the plugin service that the adjustment value has changed.
        }

        // This method is called when the reset command related to the adjustment is executed.
        protected override void RunCommand(String actionParameter)
        {
            this.Plugin.PluginEvents.RaiseEvent(
                "buttonPress"            // Event name (must match YAML files)
            );

            // this._counter = 0; // Reset the counter.
            // this.AdjustmentValueChanged(); // Notify the plugin service that the adjustment value has changed.
        }

        // Returns the adjustment value that is shown next to the dial.
        protected override String GetAdjustmentValue(String actionParameter) => this._counter.ToString();

        private static string GetCursorState()
    {
        //var h = Cursors.WaitCursor.Handle;

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

        private string oldState = "buttonPress";


        private void callback(object state)
        {
            string newState = GetCursorState();
            if (newState != oldState && newState == "65567")
            {
                this.Plugin.PluginEvents.RaiseEvent(
                    "buttonPress"            // Event name (must match YAML files)
                );
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

            Timer timer = new Timer(callback, "Some state", TimeSpan.FromMilliseconds(20), TimeSpan.FromMilliseconds(20));

            return true;
        }
    }
}
