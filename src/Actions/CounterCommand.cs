namespace Loupedeck.TutorialPlugin
{
    using System;
    using System.Runtime.InteropServices;
    using System.Net;
    using System.Net.Sockets;


    // This class implements an example command that counts button presses.

    public class CounterCommand : PluginDynamicCommand
    {
        Boolean hapticsEnabled = false;

        private string oldState = "buttonPress";
        private Timer timer;

        private int vibrateCount = 0;
        private int currentDelay = -1; // -1 means "Stop"
        private bool isOnTarget = false;

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

        private UdpClient udpServer;

        private void HapticHeartbeat()
        {
            while (true)
            {
                if (isOnTarget)
                {
                    // TARGET HIT: Constant Buzz (Oscillate)
                    // Ensure "loadingHaptic" is mapped to Logi_Oscillate in plugin.json
                    this.Plugin.PluginEvents.RaiseEvent("loadingHaptic");
                    System.Threading.Thread.Sleep(150); // Buzz duration
                }
                else if (currentDelay > 0)
                {
                    // PROXIMITY: Tick-Tick-Tick
                    // Ensure "buttonPress" is mapped to Logi_Tick
                    this.Plugin.PluginEvents.RaiseEvent("buttonPress");
                    System.Threading.Thread.Sleep(currentDelay);
                }
                else
                {
                    // IDLE: Just wait a bit to save CPU
                    System.Threading.Thread.Sleep(100);
                }
            }
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

            // Start the UDP Listener on a background thread
            Task.Run(() => StartUdpListener());

            // heartbeat for code debugging
            Task.Run(() => HapticHeartbeat());

            return true;
        }

        private void StartUdpListener()
        {
            try
            {
                udpServer = new UdpClient(11000);
                var remoteEP = new IPEndPoint(IPAddress.Any, 11000);

                while (true)
                {
                    var data = udpServer.Receive(ref remoteEP);
                    var message = System.Text.Encoding.UTF8.GetString(data);

                    if (message == "VIBE")
                    {
                        // AUDIBLE DEBUG: Beep so you know the packet arrived
                        // (This helps separate "Network Issues" from "Haptic Issues")
                        Console.Beep(1000, 200);

                        // CONTINUOUS VIBE: Fire the event 20 times rapidly
                        for (int i = 0; i < 20; i++)
                        {
                            this.Plugin.PluginEvents.RaiseEvent("buttonPress");

                            // Wait 50ms between vibes (creating a buzzing effect)
                            System.Threading.Thread.Sleep(50);
                        }
                    }

                    if (message.StartsWith("DIST:"))
                    {
                        // Parse the distance number
                        int distance = int.Parse(message.Split(':')[1]);

                        if (distance == 0)
                        {
                            isOnTarget = true;
                            currentDelay = -1;
                        }
                        else
                        {
                            isOnTarget = false;
                            // FORMULA: Closer = Faster. 
                            // Dist 1 = 100ms delay. Dist 10 = 800ms delay.
                            currentDelay = Math.Max(50, distance * 80);
                        }
                    }
                    else if (message == "CLEAR")
                    {
                        isOnTarget = false;
                        currentDelay = -1; // Stop everything
                    }
                }
            }
            catch
            {
                // Log error if needed
            }
        }

        protected override Boolean OnUnload()
        {
            udpServer?.Close();
            timer.Change(-1, -1);
            return true;
        }
    }
}
