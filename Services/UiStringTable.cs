namespace OWTrackerDesktop.Services;

internal static class UiStringTable
{
    private static readonly Dictionary<string, Dictionary<string, string>> Tables = Build();

    public static bool Contains(string languageId) =>
        Tables.ContainsKey(NormalizeId(languageId));

    public static string Get(string languageId, string key)
    {
        languageId = NormalizeId(languageId);

        if (Tables.TryGetValue(languageId, out var table) && table.TryGetValue(key, out var value))
            return value;

        if (languageId.StartsWith("es", StringComparison.Ordinal) &&
            Tables.TryGetValue("es-es", out var spanish) &&
            spanish.TryGetValue(key, out value))
            return value;

        return Tables["en"].TryGetValue(key, out value) ? value : key;
    }

    private static string NormalizeId(string id)
    {
        if (id.Equals("en-us", StringComparison.OrdinalIgnoreCase) ||
            id.Equals("en-gb", StringComparison.OrdinalIgnoreCase))
            return "en";
        return id.ToLowerInvariant();
    }

    private static Dictionary<string, Dictionary<string, string>> Build()
    {
        var en = English();
        return new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = en,
            ["fr-fr"] = Merge(en, French()),
            ["de-de"] = Merge(en, German()),
            ["it-it"] = Merge(en, Italian()),
            ["es-es"] = Merge(en, Spanish()),
            ["es-mx"] = Merge(en, Spanish()),
            ["pt-br"] = Merge(en, Portuguese()),
            ["ru-ru"] = Merge(en, Russian()),
            ["ja-jp"] = Merge(en, Japanese()),
            ["ko-kr"] = Merge(en, Korean()),
            ["zh-cn"] = Merge(en, ChineseSimplified()),
            ["zh-tw"] = Merge(en, ChineseTraditional()),
            ["pl-pl"] = Merge(en, Polish()),
            ["tr-tr"] = Merge(en, Turkish()),
            ["th-th"] = Merge(en, Thai()),
            ["uk-ua"] = Merge(en, Ukrainian()),
            ["sv-se"] = Merge(en, Swedish()),
            ["fi-fi"] = Merge(en, Finnish()),
            ["cs-cz"] = Merge(en, Czech()),
            ["hu-hu"] = Merge(en, Hungarian()),
            ["nb-no"] = Merge(en, Norwegian()),
            ["nl-nl"] = Merge(en, Dutch()),
            ["da-dk"] = Merge(en, Danish()),
        };
    }

    private static Dictionary<string, string> Merge(
        Dictionary<string, string> en,
        Dictionary<string, string> overrides)
    {
        var merged = new Dictionary<string, string>(en, StringComparer.Ordinal);
        foreach (var (key, value) in overrides)
            merged[key] = value;
        return merged;
    }

    private static Dictionary<string, string> English() => new(StringComparer.Ordinal)
    {
        ["window_title"] = "Overwatch Queue Tracker",
        ["app_subtitle"] = "Queue Notification",
        ["monitoring_active"] = "Monitoring Active",
        ["monitoring_paused"] = "Monitoring Paused",
        ["mobile_connected"] = "Mobile App Connected",
        ["mobile_disconnected"] = "Mobile App Disconnected",
        ["server_prefix"] = "Server:",
        ["game_state_section"] = "GAME STATE",
        ["display_capture"] = "DISPLAY CAPTURE",
        ["language_label"] = "LANGUAGE",
        ["start_monitoring"] = "Start Monitoring",
        ["stop_monitoring"] = "Stop Monitoring",
        ["minimize_tray"] = "Minimize to System Tray",
        ["instructions"] = "Instructions",
        ["about"] = "About",
        ["exit"] = "Exit",
        ["version_disclaimer"] = "v1.1  \u2022  Not affiliated with Blizzard Entertainment",
        ["tray_open"] = "Open",
        ["tray_start"] = "Start Monitoring",
        ["tray_stop"] = "Stop Monitoring",
        ["tray_about"] = "About",
        ["tray_exit"] = "Exit",
        ["state_idle"] = "Idle",
        ["state_searching"] = "Searching for game\u2026",
        ["state_game_found"] = "Game Found!",
        ["state_match_starting"] = "Match Starting",
        ["display_primary"] = "Primary",
        ["display_label"] = "Display {0}",
        ["qr_tooltip"] = "Scan with OW Tracker on your phone",
        ["balloon_startup_title"] = "Overwatch Queue Tracker",
        ["balloon_startup_body"] = "Monitoring started. Scan the QR code with your phone to connect.",
        ["monitoring_warning_title"] = "Monitoring warning",
        ["language_pack_title"] = "Language pack required",
        ["ws_failed_title"] = "Overwatch Queue Tracker",
        ["ws_failed_body"] = "The WebSocket server could not start (port may be in use). The phone app will not connect until this is resolved.",
        ["error_services_title"] = "Overwatch Queue Tracker - Error",
        ["error_services_body"] = "Failed to start services:\n{0}",
        ["monitoring_started_title"] = "Monitoring Started",
        ["monitoring_started_body"] = "Game monitoring is active.",
        ["monitoring_stopped_title"] = "Monitoring Stopped",
        ["monitoring_stopped_body"] = "Game monitoring is paused.",
        ["tray_minimized_title"] = "Overwatch Queue Tracker",
        ["tray_minimized_body"] = "Running in system tray. Double-click to open.",
        ["tray_summary"] = "Overwatch Queue Tracker \u2014 {0} | {1}",
        ["tray_status_active"] = "Active",
        ["tray_status_paused"] = "Paused",
        ["tray_mobile_connected"] = "Mobile connected",
        ["tray_mobile_disconnected"] = "Mobile disconnected",
        ["about_title"] = "About Overwatch Queue Tracker",
        ["about_body"] = "Overwatch Queue Tracker v1.1\n\nCompanion app for Overwatch Personal Tracker phone app (OW Tracker).\nDetects Overwatch game states and sends\nreal-time notifications to your phone.\n\nNot affiliated with Blizzard Entertainment.",
        ["confirm_exit_title"] = "Confirm exit",
        ["confirm_exit_body"] = "Exit Overwatch Queue Tracker?\n\nMonitoring and the connection to your phone will stop until you open the app again.",
        ["already_running"] = "Overwatch Queue Tracker is already running.",
        ["instructions_title"] = "How to get queue notifications on your phone",
        ["instructions_header"] = "SETUP INSTRUCTIONS",
        ["inst1_title"] = "Keep this app running",
        ["inst1_body"] = "Overwatch Queue Tracker must be open on your PC.",
        ["inst2_title"] = "Keep Overwatch visible",
        ["inst2_body"] = "Do not minimize Overwatch. The game must be visible on screen for the tracker to work.",
        ["inst3_title"] = "Same Wi\u2011Fi",
        ["inst3_body"] = "Phone and PC must be on the same Wi\u2011Fi network.",
        ["inst4_title"] = "Scan the QR code",
        ["inst4_body"] = "On your phone, open OW Tracker \u2192 Desktop tab \u2192 Scan QR code from PC, and point the camera at the QR.",
        ["inst5_title"] = "Done",
        ["inst5_body"] = "When a game is found, your phone will show a notification.",
        ["troubleshooting"] = "TROUBLESHOOTING",
        ["trouble1"] = "Make sure this app is running on your PC.",
        ["trouble2"] = "Double-check the IP address. Your router may change the IP address periodically.",
        ["trouble3"] = "Confirm your phone and PC are on the same WiFi.",
        ["trouble4"] = "Do not minimize Overwatch \u2014 it must be visible on screen to work.",
        ["trouble5"] = "Be in Fullscreen or Borderless Windowed mode in Overwatch.",
        ["got_it"] = "Got it",
    };

    private static Dictionary<string, string> French() => new(StringComparer.Ordinal)
    {
        ["app_subtitle"] = "Notification de file",
        ["monitoring_active"] = "Surveillance active",
        ["monitoring_paused"] = "Surveillance en pause",
        ["mobile_connected"] = "Appli mobile connectée",
        ["mobile_disconnected"] = "Appli mobile déconnectée",
        ["server_prefix"] = "Serveur :",
        ["game_state_section"] = "ÉTAT DU JEU",
        ["display_capture"] = "CAPTURE D'AFFICHAGE",
        ["language_label"] = "LANGUE",
        ["start_monitoring"] = "Démarrer la surveillance",
        ["stop_monitoring"] = "Arrêter la surveillance",
        ["minimize_tray"] = "Réduire dans la barre des tâches",
        ["instructions"] = "Instructions",
        ["about"] = "À propos",
        ["exit"] = "Quitter",
        ["version_disclaimer"] = "v1.1  \u2022  Non affilié à Blizzard Entertainment",
        ["tray_open"] = "Ouvrir",
        ["tray_start"] = "Démarrer la surveillance",
        ["tray_stop"] = "Arrêter la surveillance",
        ["state_idle"] = "Inactif",
        ["state_searching"] = "Recherche de jeu\u2026",
        ["state_game_found"] = "Jeu trouvé !",
        ["state_match_starting"] = "Début du match",
        ["display_primary"] = "Principal",
        ["display_label"] = "Écran {0}",
        ["qr_tooltip"] = "Scannez avec OW Tracker sur votre téléphone",
        ["balloon_startup_body"] = "Surveillance démarrée. Scannez le code QR avec votre téléphone pour se connecter.",
        ["monitoring_warning_title"] = "Avertissement de surveillance",
        ["language_pack_title"] = "Pack de langue requis",
        ["ws_failed_body"] = "Le serveur WebSocket n'a pas pu démarrer (le port est peut-être utilisé). L'appli mobile ne pourra pas se connecter tant que ce problème n'est pas résolu.",
        ["error_services_body"] = "Échec du démarrage des services :\n{0}",
        ["monitoring_started_title"] = "Surveillance démarrée",
        ["monitoring_started_body"] = "La surveillance du jeu est active.",
        ["monitoring_stopped_title"] = "Surveillance arrêtée",
        ["monitoring_stopped_body"] = "La surveillance du jeu est en pause.",
        ["tray_minimized_body"] = "Exécution dans la barre des tâches. Double-cliquez pour ouvrir.",
        ["tray_summary"] = "Overwatch Queue Tracker \u2014 {0} | {1}",
        ["tray_status_active"] = "Actif",
        ["tray_status_paused"] = "En pause",
        ["tray_mobile_connected"] = "Mobile connecté",
        ["tray_mobile_disconnected"] = "Mobile déconnecté",
        ["about_title"] = "À propos d'Overwatch Queue Tracker",
        ["about_body"] = "Overwatch Queue Tracker v1.1\n\nApplication compagnon pour OW Tracker sur téléphone.\nDétecte l'état du jeu Overwatch et envoie\ndes notifications en temps réel sur votre téléphone.\n\nNon affilié à Blizzard Entertainment.",
        ["confirm_exit_title"] = "Confirmer la fermeture",
        ["confirm_exit_body"] = "Quitter Overwatch Queue Tracker ?\n\nLa surveillance et la connexion à votre téléphone seront arrêtées jusqu'à la réouverture de l'application.",
        ["already_running"] = "Overwatch Queue Tracker est déjà en cours d'exécution.",
        ["instructions_title"] = "Recevoir des notifications de file sur votre téléphone",
        ["instructions_header"] = "INSTRUCTIONS D'INSTALLATION",
        ["inst1_title"] = "Gardez cette appli ouverte",
        ["inst1_body"] = "Overwatch Queue Tracker doit être ouvert sur votre PC.",
        ["inst2_title"] = "Gardez Overwatch visible",
        ["inst2_body"] = "Ne réduisez pas Overwatch. Le jeu doit être visible à l'écran pour que le tracker fonctionne.",
        ["inst3_title"] = "Même Wi\u2011Fi",
        ["inst3_body"] = "Le téléphone et le PC doivent être sur le même réseau Wi\u2011Fi.",
        ["inst4_title"] = "Scannez le code QR",
        ["inst4_body"] = "Sur votre téléphone, ouvrez OW Tracker \u2192 onglet Bureau \u2192 Scanner le QR du PC, et pointez la caméra vers le QR.",
        ["inst5_title"] = "Terminé",
        ["inst5_body"] = "Lorsqu'un jeu est trouvé, votre téléphone affichera une notification.",
        ["troubleshooting"] = "DÉPANNAGE",
        ["trouble1"] = "Vérifiez que cette appli est ouverte sur votre PC.",
        ["trouble2"] = "Vérifiez l'adresse IP. Votre routeur peut changer l'adresse IP périodiquement.",
        ["trouble3"] = "Confirmez que votre téléphone et votre PC sont sur le même Wi\u2011Fi.",
        ["trouble4"] = "Ne réduisez pas Overwatch \u2014 il doit être visible à l'écran.",
        ["trouble5"] = "Utilisez le mode Plein écran ou Fenêtré sans bordure dans Overwatch.",
        ["got_it"] = "Compris",
    };

    private static Dictionary<string, string> German() => new(StringComparer.Ordinal)
    {
        ["app_subtitle"] = "Wartebenachrichtigung",
        ["monitoring_active"] = "Überwachung aktiv",
        ["monitoring_paused"] = "Überwachung pausiert",
        ["mobile_connected"] = "Mobile App verbunden",
        ["mobile_disconnected"] = "Mobile App getrennt",
        ["server_prefix"] = "Server:",
        ["game_state_section"] = "SPIELSTATUS",
        ["display_capture"] = "BILDSCHIRMAUFNAHME",
        ["language_label"] = "SPRACHE",
        ["start_monitoring"] = "Überwachung starten",
        ["stop_monitoring"] = "Überwachung stoppen",
        ["minimize_tray"] = "In Taskleiste minimieren",
        ["instructions"] = "Anleitung",
        ["about"] = "Info",
        ["exit"] = "Beenden",
        ["version_disclaimer"] = "v1.1  \u2022  Nicht mit Blizzard Entertainment verbunden",
        ["tray_open"] = "Öffnen",
        ["state_idle"] = "Leerlauf",
        ["state_searching"] = "Suche Spiel\u2026",
        ["state_game_found"] = "Spiel gefunden!",
        ["state_match_starting"] = "Match startet",
        ["display_primary"] = "Primär",
        ["display_label"] = "Anzeige {0}",
        ["qr_tooltip"] = "Mit OW Tracker auf dem Telefon scannen",
        ["balloon_startup_body"] = "Überwachung gestartet. QR-Code mit dem Telefon scannen, um zu verbinden.",
        ["language_pack_title"] = "Sprachpaket erforderlich",
        ["ws_failed_body"] = "Der WebSocket-Server konnte nicht starten (Port möglicherweise belegt). Die Mobile App kann nicht verbinden, bis dies behoben ist.",
        ["monitoring_started_title"] = "Überwachung gestartet",
        ["monitoring_started_body"] = "Spielüberwachung ist aktiv.",
        ["monitoring_stopped_title"] = "Überwachung gestoppt",
        ["monitoring_stopped_body"] = "Spielüberwachung ist pausiert.",
        ["tray_minimized_body"] = "Läuft in der Taskleiste. Doppelklick zum Öffnen.",
        ["tray_status_active"] = "Aktiv",
        ["tray_status_paused"] = "Pausiert",
        ["tray_mobile_connected"] = "Mobil verbunden",
        ["tray_mobile_disconnected"] = "Mobil getrennt",
        ["about_title"] = "Über Overwatch Queue Tracker",
        ["confirm_exit_title"] = "Beenden bestätigen",
        ["confirm_exit_body"] = "Overwatch Queue Tracker beenden?\n\nÜberwachung und Telefonverbindung werden gestoppt, bis Sie die App erneut öffnen.",
        ["already_running"] = "Overwatch Queue Tracker läuft bereits.",
        ["instructions_title"] = "Wartebenachrichtigungen auf dem Telefon erhalten",
        ["instructions_header"] = "EINRICHTUNG",
        ["inst1_title"] = "App geöffnet lassen",
        ["inst1_body"] = "Overwatch Queue Tracker muss auf dem PC geöffnet sein.",
        ["inst2_title"] = "Overwatch sichtbar lassen",
        ["inst2_body"] = "Overwatch nicht minimieren. Das Spiel muss auf dem Bildschirm sichtbar sein.",
        ["inst3_title"] = "Gleiches WLAN",
        ["inst3_body"] = "Telefon und PC müssen im gleichen WLAN sein.",
        ["inst4_title"] = "QR-Code scannen",
        ["inst4_body"] = "Auf dem Telefon OW Tracker \u2192 Desktop \u2192 QR vom PC scannen und Kamera auf den QR richten.",
        ["inst5_title"] = "Fertig",
        ["inst5_body"] = "Wenn ein Spiel gefunden wird, zeigt Ihr Telefon eine Benachrichtigung.",
        ["troubleshooting"] = "FEHLERBEHEBUNG",
        ["got_it"] = "Verstanden",
    };

    // Additional languages use shorter overrides for key UI; English fills gaps.
    private static Dictionary<string, string> Italian() => new(StringComparer.Ordinal)
    {
        ["app_subtitle"] = "Notifica di coda",
        ["monitoring_active"] = "Monitoraggio attivo",
        ["monitoring_paused"] = "Monitoraggio in pausa",
        ["mobile_connected"] = "App mobile connessa",
        ["mobile_disconnected"] = "App mobile disconnessa",
        ["language_label"] = "LINGUA",
        ["start_monitoring"] = "Avvia monitoraggio",
        ["stop_monitoring"] = "Ferma monitoraggio",
        ["state_searching"] = "Ricerca partita\u2026",
        ["state_game_found"] = "Partita trovata!",
        ["instructions"] = "Istruzioni",
        ["about"] = "Informazioni",
        ["exit"] = "Esci",
        ["got_it"] = "Capito",
    };

    private static Dictionary<string, string> Spanish() => new(StringComparer.Ordinal)
    {
        ["app_subtitle"] = "Notificación de cola",
        ["monitoring_active"] = "Monitoreo activo",
        ["monitoring_paused"] = "Monitoreo en pausa",
        ["mobile_connected"] = "App móvil conectada",
        ["mobile_disconnected"] = "App móvil desconectada",
        ["language_label"] = "IDIOMA",
        ["start_monitoring"] = "Iniciar monitoreo",
        ["stop_monitoring"] = "Detener monitoreo",
        ["state_searching"] = "Buscando partida\u2026",
        ["state_game_found"] = "¡Partida encontrada!",
        ["instructions"] = "Instrucciones",
        ["about"] = "Acerca de",
        ["exit"] = "Salir",
        ["got_it"] = "Entendido",
    };

    private static Dictionary<string, string> Portuguese() => new(StringComparer.Ordinal)
    {
        ["monitoring_active"] = "Monitoramento ativo",
        ["monitoring_paused"] = "Monitoramento pausado",
        ["mobile_connected"] = "App móvel conectado",
        ["mobile_disconnected"] = "App móvel desconectado",
        ["language_label"] = "IDIOMA",
        ["start_monitoring"] = "Iniciar monitoramento",
        ["stop_monitoring"] = "Parar monitoramento",
        ["state_searching"] = "Procurando jogo\u2026",
        ["state_game_found"] = "Jogo encontrado!",
        ["got_it"] = "Entendi",
    };

    private static Dictionary<string, string> Russian() => new(StringComparer.Ordinal)
    {
        ["monitoring_active"] = "Мониторинг активен",
        ["monitoring_paused"] = "Мониторинг приостановлен",
        ["mobile_connected"] = "Мобильное приложение подключено",
        ["mobile_disconnected"] = "Мобильное приложение отключено",
        ["language_label"] = "ЯЗЫК",
        ["start_monitoring"] = "Запустить мониторинг",
        ["stop_monitoring"] = "Остановить мониторинг",
        ["state_searching"] = "Поиск игры\u2026",
        ["state_game_found"] = "Игра найдена!",
        ["got_it"] = "Понятно",
    };

    private static Dictionary<string, string> Japanese() => new(StringComparer.Ordinal)
    {
        ["monitoring_active"] = "監視中",
        ["monitoring_paused"] = "監視一時停止",
        ["mobile_connected"] = "モバイルアプリ接続済み",
        ["mobile_disconnected"] = "モバイルアプリ未接続",
        ["language_label"] = "言語",
        ["start_monitoring"] = "監視を開始",
        ["stop_monitoring"] = "監視を停止",
        ["state_searching"] = "ゲームを検索中\u2026",
        ["state_game_found"] = "ゲーム発見！",
        ["got_it"] = "了解",
    };

    private static Dictionary<string, string> Korean() => new(StringComparer.Ordinal)
    {
        ["monitoring_active"] = "모니터링 활성",
        ["monitoring_paused"] = "모니터링 일시정지",
        ["mobile_connected"] = "모바일 앱 연결됨",
        ["mobile_disconnected"] = "모바일 앱 연결 안 됨",
        ["language_label"] = "언어",
        ["start_monitoring"] = "모니터링 시작",
        ["stop_monitoring"] = "모니터링 중지",
        ["state_searching"] = "게임 검색 중\u2026",
        ["state_game_found"] = "게임 발견!",
        ["got_it"] = "확인",
    };

    private static Dictionary<string, string> ChineseSimplified() => new(StringComparer.Ordinal)
    {
        ["monitoring_active"] = "监控中",
        ["monitoring_paused"] = "监控已暂停",
        ["mobile_connected"] = "手机应用已连接",
        ["mobile_disconnected"] = "手机应用未连接",
        ["language_label"] = "语言",
        ["start_monitoring"] = "开始监控",
        ["stop_monitoring"] = "停止监控",
        ["state_searching"] = "正在搜索比赛\u2026",
        ["state_game_found"] = "找到比赛！",
        ["got_it"] = "知道了",
    };

    private static Dictionary<string, string> ChineseTraditional() => new(StringComparer.Ordinal)
    {
        ["monitoring_active"] = "監控中",
        ["monitoring_paused"] = "監控已暫停",
        ["mobile_connected"] = "手機應用程式已連線",
        ["mobile_disconnected"] = "手機應用程式未連線",
        ["language_label"] = "語言",
        ["start_monitoring"] = "開始監控",
        ["stop_monitoring"] = "停止監控",
        ["state_searching"] = "正在搜尋對戰\u2026",
        ["state_game_found"] = "找到對戰！",
        ["got_it"] = "知道了",
    };

    private static Dictionary<string, string> Polish() => new(StringComparer.Ordinal)
    {
        ["monitoring_active"] = "Monitorowanie aktywne",
        ["monitoring_paused"] = "Monitorowanie wstrzymane",
        ["language_label"] = "JĘZYK",
        ["state_searching"] = "Szukanie gry\u2026",
        ["state_game_found"] = "Gra znaleziona!",
        ["got_it"] = "Rozumiem",
    };

    private static Dictionary<string, string> Turkish() => new(StringComparer.Ordinal)
    {
        ["monitoring_active"] = "İzleme aktif",
        ["monitoring_paused"] = "İzleme duraklatıldı",
        ["language_label"] = "DİL",
        ["state_searching"] = "Oyun aranıyor\u2026",
        ["state_game_found"] = "Oyun bulundu!",
        ["got_it"] = "Anladım",
    };

    private static Dictionary<string, string> Thai() => new(StringComparer.Ordinal)
    {
        ["monitoring_active"] = "กำลังตรวจสอบ",
        ["monitoring_paused"] = "หยุดตรวจสอบชั่วคราว",
        ["language_label"] = "ภาษา",
        ["state_searching"] = "กำลังค้นหาเกม\u2026",
        ["state_game_found"] = "พบเกมแล้ว!",
        ["got_it"] = "เข้าใจแล้ว",
    };

    private static Dictionary<string, string> Ukrainian() => new(StringComparer.Ordinal)
    {
        ["monitoring_active"] = "Моніторинг активний",
        ["monitoring_paused"] = "Моніторинг призупинено",
        ["language_label"] = "МОВА",
        ["state_searching"] = "Пошук ігри\u2026",
        ["state_game_found"] = "Гру знайдено!",
        ["got_it"] = "Зрозуміло",
    };

    private static Dictionary<string, string> Swedish() => new(StringComparer.Ordinal)
    {
        ["monitoring_active"] = "Övervakning aktiv",
        ["monitoring_paused"] = "Övervakning pausad",
        ["language_label"] = "SPRÅK",
        ["state_searching"] = "Söker spel\u2026",
        ["state_game_found"] = "Spel hittat!",
        ["got_it"] = "Okej",
    };

    private static Dictionary<string, string> Finnish() => new(StringComparer.Ordinal)
    {
        ["monitoring_active"] = "Valvonta käynnissä",
        ["monitoring_paused"] = "Valvonta keskeytetty",
        ["language_label"] = "KIELI",
        ["state_searching"] = "Etsii peliä\u2026",
        ["state_game_found"] = "Peli löytyi!",
        ["got_it"] = "Selvä",
    };

    private static Dictionary<string, string> Czech() => new(StringComparer.Ordinal)
    {
        ["monitoring_active"] = "Monitorování aktivní",
        ["monitoring_paused"] = "Monitorování pozastaveno",
        ["language_label"] = "JAZYK",
        ["state_searching"] = "Hledání hry\u2026",
        ["state_game_found"] = "Hra nalezena!",
        ["got_it"] = "Rozumím",
    };

    private static Dictionary<string, string> Hungarian() => new(StringComparer.Ordinal)
    {
        ["monitoring_active"] = "Figyelés aktív",
        ["monitoring_paused"] = "Figyelés szünetel",
        ["language_label"] = "NYELV",
        ["state_searching"] = "Játék keresése\u2026",
        ["state_game_found"] = "Játék megtalálva!",
        ["got_it"] = "Értem",
    };

    private static Dictionary<string, string> Norwegian() => new(StringComparer.Ordinal)
    {
        ["monitoring_active"] = "Overvåking aktiv",
        ["monitoring_paused"] = "Overvåking pausert",
        ["language_label"] = "SPRÅK",
        ["state_searching"] = "Søker spill\u2026",
        ["state_game_found"] = "Spill funnet!",
        ["got_it"] = "Greit",
    };

    private static Dictionary<string, string> Dutch() => new(StringComparer.Ordinal)
    {
        ["monitoring_active"] = "Monitoring actief",
        ["monitoring_paused"] = "Monitoring gepauzeerd",
        ["language_label"] = "TAAL",
        ["state_searching"] = "Spel zoeken\u2026",
        ["state_game_found"] = "Spel gevonden!",
        ["got_it"] = "Begrepen",
    };

    private static Dictionary<string, string> Danish() => new(StringComparer.Ordinal)
    {
        ["monitoring_active"] = "Overvågning aktiv",
        ["monitoring_paused"] = "Overvågning sat på pause",
        ["language_label"] = "SPROG",
        ["state_searching"] = "Søger spil\u2026",
        ["state_game_found"] = "Spil fundet!",
        ["got_it"] = "Forstået",
    };
}
