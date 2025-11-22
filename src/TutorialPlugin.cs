namespace Loupedeck.TutorialPlugin
{
    using System;
    using System.Threading;

    // This class contains the plugin-level logic of the Loupedeck plugin.

    public class TutorialPlugin : Plugin
    {
        // Gets a value indicating whether this is an API-only plugin.
        public override Boolean UsesApplicationApiOnly => true;

        // Gets a value indicating whether this is a Universal plugin or an Application plugin.
        public override Boolean HasNoApplication => true;

        // Initializes a new instance of the plugin class.
        public TutorialPlugin()
        {
            // Initialize the plugin log.
            PluginLog.Init(this.Log);

            // Initialize the plugin resources.
            PluginResources.Init(this.Assembly);
        }

        // This method is called when the plugin is loaded.
        public override void Load()
        {
           
        }

        // This method is called when the plugin is unloaded.
        public override void Unload()
        {
        }
    }
}
