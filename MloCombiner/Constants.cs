namespace MloCombiner {
    public class Constants {

        public const string DoneString = "Done!";
        
        public const string Motd = @"
___  ____       _____                 _     _                        
|  \/  | |     /  __ \               | |   (_)                       
| .  . | | ___ | /  \/ ___  _ __ ___ | |__  _ _ __   ___ _ __        
| |\/| | |/ _ \| |    / _ \| '_ ` _ \| '_ \| | '_ \ / _ \ '__|       
| |  | | | (_) | \__/\ (_) | | | | | | |_) | | | | |  __/ |          
\_|  |_/_|\___/ \____/\___/|_| |_| |_|_.__/|_|_| |_|\___|_|          
";
        
        public static readonly string[] KnownMetaFiles = { "__resource.lua", "fxmanifest.lua" };

        public const string ManifestContent = @"
fx_version 'bodacious'
game 'gta5'

author 'prp'
version '1.0.0'

this_is_a_map 'yes'
";

    }
}