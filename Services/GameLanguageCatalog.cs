using OWTrackerDesktop.Models;

namespace OWTrackerDesktop.Services;

public static class GameLanguageCatalog
{
  public static GameLanguage Default => All[0];

  public static IReadOnlyList<GameLanguage> All { get; } = BuildAll();

  public static GameLanguage? FindById(string id) =>
      All.FirstOrDefault(l => l.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

  private static IReadOnlyList<GameLanguage> BuildAll()
  {
    var languages = new List<GameLanguage>
    {
      Define("en", "English", "en-US",
        gameFound: ["GAME FOUND", "MATCH FOUND"],
        searching: ["SEARCHING", "SEARCHING FOR GAME", "SEARCH FOR GAME"],
        cancel: ["CANCEL"],
        matchStarting: ["ENTERING GAME", "ENTERING PREGAME", "PREGAME", "ASSEMBLING HEROES"]),

      Define("fr-fr", "French", "fr-FR",
        gameFound: ["JEU TROUVÉ", "JEU TROUVE", "PARTIE TROUVÉE", "PARTIE TROUVEE", "MATCH TROUVÉ"],
        searching: ["RECHERCHE", "RECHERCHE DE JEU", "EN RECHERCHE"],
        cancel: ["ANNULER"],
        matchStarting: ["ENTRÉE EN JEU", "ENTREE EN JEU", "ENTRÉE DANS LE JEU", "PRÉPARTIE", "PREPARTIE"]),

      Define("de-de", "German", "de-DE",
        gameFound: ["SPIEL GEFUNDEN", "SPIEL GEFUNDEN!", "MATCH GEFUNDEN"],
        searching: ["SUCHEN", "SUCHE", "SPIEL SUCHEN", "SUCHE SPIEL"],
        cancel: ["ABBRECHEN"],
        matchStarting: ["SPIEL BETRETEN", "SPIEL WIRD BETRETEN", "VORSPIEL", "HELDEN WERDEN ZUSAMMENGESTELLT"]),

      Define("it-it", "Italian", "it-IT",
        gameFound: ["PARTITA TROVATA", "MATCH TROVATO", "GIOCO TROVATO"],
        searching: ["RICERCA", "CERCANDO", "RICERCA PARTITA"],
        cancel: ["ANNULLA", "ANNULLARE"],
        matchStarting: ["ENTRATA NEL GIOCO", "ACCESSO AL GIOCO", "PREGAME", "ASSEMBLAGGIO EROI"]),

      Define("es-es", "Spanish (Spain)", "es-ES",
        gameFound: ["PARTIDA ENCONTRADA", "JUEGO ENCONTRADO", "MATCH ENCONTRADO"],
        searching: ["BUSCANDO", "BUSCANDO PARTIDA", "BUSCANDO JUEGO"],
        cancel: ["CANCELAR"],
        matchStarting: ["ENTRANDO AL JUEGO", "ENTRANDO EN EL JUEGO", "PREGAME", "REUNIENDO HÉROES", "REUNIENDO HEROES"]),

      Define("es-mx", "Spanish (Latin America)", "es-MX",
        gameFound: ["PARTIDA ENCONTRADA", "JUEGO ENCONTRADO", "MATCH ENCONTRADO"],
        searching: ["BUSCANDO", "BUSCANDO PARTIDA", "BUSCANDO JUEGO"],
        cancel: ["CANCELAR"],
        matchStarting: ["ENTRANDO AL JUEGO", "ENTRANDO EN EL JUEGO", "PREGAME", "REUNIENDO HÉROES", "REUNIENDO HEROES"]),

      Define("pt-br", "Portuguese (Brazil)", "pt-BR",
        gameFound: ["JOGO ENCONTRADO", "PARTIDA ENCONTRADA", "MATCH ENCONTRADO"],
        searching: ["PROCURANDO", "BUSCANDO", "PROCURANDO JOGO"],
        cancel: ["CANCELAR"],
        matchStarting: ["ENTRANDO NO JOGO", "ENTRANDO EM JOGO", "PRÉ-JOGO", "PRE-JOGO", "MONTANDO HERÓIS", "MONTANDO HEROIS"]),

      Define("ru-ru", "Russian", "ru-RU",
        gameFound: ["ИГРА НАЙДЕНА", "МАТЧ НАЙДЕН", "ИГРА НАЙДЕНА!"],
        searching: ["ПОИСК", "ПОИСК ИГРЫ", "ИЩЕМ ИГРУ"],
        cancel: ["ОТМЕНА", "ОТМЕНИТЬ"],
        matchStarting: ["ВХОД В ИГРУ", "ПОДГОТОВКА", "СБОР ГЕРОЕВ"]),

      Define("ja-jp", "Japanese", "ja-JP",
        gameFound: ["マッチ成立", "ゲーム発見", "対戦成立", "ゲームが見つかりました"],
        searching: ["検索中", "検索", "マッチを検索中"],
        cancel: ["キャンセル"],
        matchStarting: ["ゲームに参加", "ゲーム参加中", "プレゲーム", "ヒーロー集合"]),

      Define("ko-kr", "Korean", "ko-KR",
        gameFound: ["게임을 찾았습니다", "게임 발견", "매치 발견", "게임 발견!"],
        searching: ["검색 중", "검색중", "게임 검색 중", "게임 검색중"],
        cancel: ["취소"],
        matchStarting: ["게임 참가 중", "게임 입장", "프리게임", "영웅 집결"]),

      Define("zh-cn", "Simplified Chinese", "zh-CN",
        gameFound: ["比赛已找到", "找到比赛", "已找到比赛", "对局已找到"],
        searching: ["正在搜索", "搜索中", "正在搜寻"],
        cancel: ["取消"],
        matchStarting: ["正在进入游戏", "进入游戏", "赛前准备", "集结英雄"]),

      Define("zh-tw", "Traditional Chinese", "zh-TW",
        gameFound: ["找到對戰", "對戰已找到", "已找到對戰", "比賽已找到"],
        searching: ["正在搜尋", "搜尋中", "正在搜索"],
        cancel: ["取消"],
        matchStarting: ["正在進入遊戲", "進入遊戲", "賽前準備", "集結英雄"]),

      Define("pl-pl", "Polish", "pl-PL",
        gameFound: ["GRA ZNALEZIONA", "MECZ ZNALEZIONY", "GRA ZNALEZIONA!"],
        searching: ["WYSZUKIWANIE", "SZUKANIE", "WYSZUKIWANIE GRY"],
        cancel: ["ANULUJ"],
        matchStarting: ["WCHODZENIE DO GRY", "WEJŚCIE DO GRY", "WEJSCIE DO GRY", "PRZEDGRA", "ZBIERANIE BOHATERÓW", "ZBIERANIE BOHATEROW"]),

      Define("tr-tr", "Turkish", "tr-TR",
        gameFound: ["OYUN BULUNDU", "MAÇ BULUNDU", "ESLESME BULUNDU"],
        searching: ["ARANIYOR", "ARAMA", "OYUN ARANIYOR"],
        cancel: ["İPTAL", "IPTAL"],
        matchStarting: ["OYUNA GİRİLİYOR", "OYUNA GIRILIYOR", "ÖN OYUN", "ON OYUN", "KAHRAMANLAR TOPLANIYOR"]),

      Define("th-th", "Thai", "th-TH",
        gameFound: ["พบเกมแล้ว", "พบแมตช์แล้ว", "พบการแข่งขันแล้ว"],
        searching: ["กำลังค้นหา", "กำลังหา", "ค้นหาเกม"],
        cancel: ["ยกเลิก"],
        matchStarting: ["กำลังเข้าเกม", "เข้าเกม", "ก่อนเกม"]),

      Define("uk-ua", "Ukrainian", "uk-UA",
        gameFound: ["ГРУ ЗНАЙДЕНО", "МАТЧ ЗНАЙДЕНО", "ІГРУ ЗНАЙДЕНО"],
        searching: ["ПОШУК", "ПОШУК ІГРИ", "ШУКАЄМО ІГРУ", "ШУКАЕМО ІГРУ"],
        cancel: ["СКАСУВАТИ", "СКАСУВАННЯ"],
        matchStarting: ["ВХІД У ГРУ", "ВХІД В ІГРУ", "ПЕРЕДІГРА", "ЗБІР ГЕРОЇВ"]),

      Define("sv-se", "Swedish", "sv-SE",
        gameFound: ["MATCH HITTAD", "SPEL HITTAT", "SPELET HITTAT"],
        searching: ["SÖKER", "SOKER", "SÖKER SPEL", "SOKER SPEL"],
        cancel: ["AVBRYT"],
        matchStarting: ["GÅR IN I SPELET", "GAR IN I SPELET", "FÖRSPEL", "FORSPEL"]),

      Define("fi-fi", "Finnish", "fi-FI",
        gameFound: ["PELI LÖYTYI", "PELI LOYTYI", "OTTELU LÖYTYI", "OTTELU LOYTYI"],
        searching: ["ETSIÄÄN", "ETSITAAN", "HAKU", "PELIN HAKU"],
        cancel: ["PERUUTA"],
        matchStarting: ["ASTUTAAN PELIIN", "ASTETAAN PELIIN", "ESIPELI"]),

      Define("cs-cz", "Czech", "cs-CZ",
        gameFound: ["HRA NALEZENA", "ZÁPAS NALEZEN", "ZAPAS NALEZEN"],
        searching: ["VYHLEDÁVÁNÍ", "VYHLEDAVANI", "HLEDÁNÍ", "HLEDANI"],
        cancel: ["ZRUŠIT", "ZRUSIT"],
        matchStarting: ["VSTUP DO HRY", "VSTUPUJI DO HRY", "PŘEDHRA", "PREDHRA"]),

      Define("hu-hu", "Hungarian", "hu-HU",
        gameFound: ["JÁTÉK MEGTALÁLVA", "JATEK MEGTALALVA", "MÉRKŐZÉS MEGTALÁLVA", "MERKOZES MEGTALALVA"],
        searching: ["KERESÉS", "KERESES", "JÁTÉK KERESÉSE", "JATEK KERESESE"],
        cancel: ["MÉGSE", "MEGSE"],
        matchStarting: ["BELÉPÉS A JÁTÉKBA", "BELEPES A JATEKBA", "ELŐJÁTÉK", "ELOJATEK"]),

      Define("nb-no", "Norwegian", "nb-NO",
        gameFound: ["KAMP FUNNET", "SPILL FUNNET", "MATCH FUNNET"],
        searching: ["SØKER", "SOKER", "SØKER SPILL", "SOKER SPILL"],
        cancel: ["AVBRYT"],
        matchStarting: ["GÅR INN I SPILLET", "GAR INN I SPILLET", "FORSPILL"]),

      Define("nl-nl", "Dutch", "nl-NL",
        gameFound: ["WEDSTRIJD GEVONDEN", "SPEL GEVONDEN", "MATCH GEVONDEN"],
        searching: ["ZOEKEN", "SPEL ZOEKEN", "ZOEKEN NAAR SPEL"],
        cancel: ["ANNULEREN"],
        matchStarting: ["SPEL BETREDEN", "GAAT NAAR SPEL", "VOORSPEL"]),

      Define("da-dk", "Danish", "da-DK",
        gameFound: ["KAMP FUNDET", "SPIL FUNDET", "MATCH FUNDET"],
        searching: ["SØGER", "SOGER", "SØGER SPIL", "SOGER SPIL"],
        cancel: ["ANNULLER"],
        matchStarting: ["GÅR IND I SPILLET", "GAR IND I SPILLET", "FORSPIL"]),
    };

    return languages;
  }

  private static GameLanguage Define(
      string id,
      string displayName,
      string ocrLanguageTag,
      string[] gameFound,
      string[] searching,
      string[] cancel,
      string[] matchStarting)
  {
    return new GameLanguage
    {
      Id = id,
      DisplayName = displayName,
      OcrLanguageTag = ocrLanguageTag,
      GameFoundTokens = OcrTextNormalizer.NormalizeTokens(gameFound),
      SearchingTokens = OcrTextNormalizer.NormalizeTokens(searching),
      CancelTokens = OcrTextNormalizer.NormalizeTokens(cancel),
      MatchStartingTokens = OcrTextNormalizer.NormalizeTokens(matchStarting),
    };
  }
}
