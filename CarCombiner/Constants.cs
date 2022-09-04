namespace CarCombiner {
    public static class Constants {

        public const string DoneString = "Done!";

        public const string Motd = @"
 _____            _____                 _     _                      
/  __ \          /  __ \               | |   (_)                     
| /  \/ __ _ _ __| /  \/ ___  _ __ ___ | |__  _ _ __   ___ _ __      
| |    / _` | '__| |    / _ \| '_ ` _ \| '_ \| | '_ \ / _ \ '__|     
| \__/\ (_| | |  | \__/\ (_) | | | | | | |_) | | | | |  __/ |        
 \____/\__,_|_|   \____/\___/|_| |_| |_|_.__/|_|_| |_|\___|_|        
                                                                     
                                                                     
 _             _                        _   _                        
| |           | |                      | | | |                       
| |__  _   _  | |     ___  ___  _ __   | |_| | ___  _ __  _ __   ___ 
| '_ \| | | | | |    / _ \/ _ \| '_ \  |  _  |/ _ \| '_ \| '_ \ / _ \
| |_) | |_| | | |___|  __/ (_) | | | | | | | | (_) | |_) | |_) |  __/
|_.__/ \__, | \_____/\___|\___/|_| |_| \_| |_/\___/| .__/| .__/ \___|
        __/ |                                      | |   | |         
       |___/                                       |_|   |_|                    
";

        public static readonly string[] KnownMetaFiles = { "__resource.lua", "fxmanifest.lua", "vehicles.meta", "carvariations.meta", "carcols.meta", "handling.meta", "vehiclelayouts.meta", "dlctext.meta" };

        public const string ManifestContent = @"
resource_manifest_version '77731fab-63ca-442c-a67b-abc70f28dfa5'
 
files {
    'vehicles.meta',
    'carvariations.meta',
    'carcols.meta',
    'handling.meta',
    'vehiclelayouts.meta',
}

data_file 'HANDLING_FILE' 'handling.meta'
data_file 'VEHICLE_METADATA_FILE' 'vehicles.meta'
data_file 'CARCOLS_FILE' 'carcols.meta'
data_file 'VEHICLE_VARIATION_FILE' 'carvariations.meta'
data_file 'VEHICLE_LAYOUTS_FILE' 'vehiclelayouts.meta'
";

        public const string CarcolsScaffolding = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><CVehicleModelInfoVarGlobal><Kits></Kits><Lights></Lights><Sirens></Sirens></CVehicleModelInfoVarGlobal>";

    }
}