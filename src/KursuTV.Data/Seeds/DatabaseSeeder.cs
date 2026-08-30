using Microsoft.EntityFrameworkCore;
using KursuTV.Data.Context;
using KursuTV.Data.Entities;
using KursuTV.Data.Enums;

namespace KursuTV.Data.Seeds;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        // â”€â”€ Åehirler â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        var cityCount = context.Cities.Count();
        if (cityCount < 81)
        {
            var existingNames = context.Cities.Select(c => c.Name).ToHashSet();
            var allCities = GetCities();

            // Eksik ÅŸehirleri ID olmadan ekle (sequence otomatik atar)
            var missingCities = allCities
                .Where(c => !existingNames.Contains(c.Name))
                .Select(c => new City { Name = c.Name, Slug = c.Slug, PlateCode = c.PlateCode })
                .ToList();

            if (missingCities.Count > 0)
            {
                await context.Cities.AddRangeAsync(missingCities);
                await context.SaveChangesAsync();
            }
        }

        // â”€â”€ Ä°lÃ§eler â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        // Ä°lÃ§esi olmayan ÅŸehirlere ilÃ§e ekle
        var plateToDistricts = GetDistrictsByPlate();
        var allDbCities = context.Cities.AsNoTracking().ToList();
        var existingDistrictCityIds = context.Districts
            .Select(d => d.CityId).Distinct().ToHashSet();

        var citiesNeedingDistricts = allDbCities
            .Where(c => !existingDistrictCityIds.Contains(c.Id))
            .ToList();

        if (citiesNeedingDistricts.Count > 0)
        {
            var newDistricts = new List<District>();
            foreach (var city in citiesNeedingDistricts)
            {
                if (plateToDistricts.TryGetValue(city.PlateCode, out var districtNames))
                {
                    foreach (var name in districtNames)
                        newDistricts.Add(new District { CityId = city.Id, Name = name, Slug = Slugify(name) });
                }
            }
            if (newDistricts.Count > 0)
            {
                await context.Districts.AddRangeAsync(newDistricts);
                await context.SaveChangesAsync();
            }
        }

        // â”€â”€ BranÅŸlar â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        var branchCount = context.Branches.Count();
        if (branchCount < 10)
        {
            var existingBranchNames = context.Branches.Select(b => b.Name).ToHashSet();
            var allBranches = GetBranches();
            var missingBranches = allBranches
                .Where(b => !existingBranchNames.Contains(b.Name))
                .Select(b => new Branch
                {
                    Name = b.Name, Slug = b.Slug, Category = b.Category,
                    IsPopular = b.IsPopular, DisplayOrder = b.DisplayOrder
                })
                .ToList();

            if (missingBranches.Count > 0)
            {
                await context.Branches.AddRangeAsync(missingBranches);
                await context.SaveChangesAsync();
            }
        }
    }

    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
    // ÅEHÄ°RLER â€” TÃ¼rkiye 81 Ä°l
    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
    private static List<City> GetCities() => new()
    {
        new() { Id=1,  Name="Adana",           Slug="adana",           PlateCode=1  },
        new() { Id=2,  Name="AdÄ±yaman",        Slug="adiyaman",        PlateCode=2  },
        new() { Id=3,  Name="Afyonkarahisar",  Slug="afyonkarahisar",  PlateCode=3  },
        new() { Id=4,  Name="AÄŸrÄ±",            Slug="agri",            PlateCode=4  },
        new() { Id=5,  Name="Amasya",          Slug="amasya",          PlateCode=5  },
        new() { Id=6,  Name="Ankara",          Slug="ankara",          PlateCode=6  },
        new() { Id=7,  Name="Antalya",         Slug="antalya",         PlateCode=7  },
        new() { Id=8,  Name="Artvin",          Slug="artvin",          PlateCode=8  },
        new() { Id=9,  Name="AydÄ±n",           Slug="aydin",           PlateCode=9  },
        new() { Id=10, Name="BalÄ±kesir",       Slug="balikesir",       PlateCode=10 },
        new() { Id=11, Name="Bilecik",         Slug="bilecik",         PlateCode=11 },
        new() { Id=12, Name="BingÃ¶l",          Slug="bingol",          PlateCode=12 },
        new() { Id=13, Name="Bitlis",          Slug="bitlis",          PlateCode=13 },
        new() { Id=14, Name="Bolu",            Slug="bolu",            PlateCode=14 },
        new() { Id=15, Name="Burdur",          Slug="burdur",          PlateCode=15 },
        new() { Id=16, Name="Bursa",           Slug="bursa",           PlateCode=16 },
        new() { Id=17, Name="Ã‡anakkale",       Slug="canakkale",       PlateCode=17 },
        new() { Id=18, Name="Ã‡ankÄ±rÄ±",         Slug="cankiri",         PlateCode=18 },
        new() { Id=19, Name="Ã‡orum",           Slug="corum",           PlateCode=19 },
        new() { Id=20, Name="Denizli",         Slug="denizli",         PlateCode=20 },
        new() { Id=21, Name="DiyarbakÄ±r",      Slug="diyarbakir",      PlateCode=21 },
        new() { Id=22, Name="Edirne",          Slug="edirne",          PlateCode=22 },
        new() { Id=23, Name="ElazÄ±ÄŸ",          Slug="elazig",          PlateCode=23 },
        new() { Id=24, Name="Erzincan",        Slug="erzincan",        PlateCode=24 },
        new() { Id=25, Name="Erzurum",         Slug="erzurum",         PlateCode=25 },
        new() { Id=26, Name="EskiÅŸehir",       Slug="eskisehir",       PlateCode=26 },
        new() { Id=27, Name="Gaziantep",       Slug="gaziantep",       PlateCode=27 },
        new() { Id=28, Name="Giresun",         Slug="giresun",         PlateCode=28 },
        new() { Id=29, Name="GÃ¼mÃ¼ÅŸhane",       Slug="gumushane",       PlateCode=29 },
        new() { Id=30, Name="Hakkari",         Slug="hakkari",         PlateCode=30 },
        new() { Id=31, Name="Hatay",           Slug="hatay",           PlateCode=31 },
        new() { Id=32, Name="Isparta",         Slug="isparta",         PlateCode=32 },
        new() { Id=33, Name="Mersin",          Slug="mersin",          PlateCode=33 },
        new() { Id=34, Name="Ä°stanbul",        Slug="istanbul",        PlateCode=34 },
        new() { Id=35, Name="Ä°zmir",           Slug="izmir",           PlateCode=35 },
        new() { Id=36, Name="Kars",            Slug="kars",            PlateCode=36 },
        new() { Id=37, Name="Kastamonu",       Slug="kastamonu",       PlateCode=37 },
        new() { Id=38, Name="Kayseri",         Slug="kayseri",         PlateCode=38 },
        new() { Id=39, Name="KÄ±rklareli",      Slug="kirklareli",      PlateCode=39 },
        new() { Id=40, Name="KÄ±rÅŸehir",        Slug="kirsehir",        PlateCode=40 },
        new() { Id=41, Name="Kocaeli",         Slug="kocaeli",         PlateCode=41 },
        new() { Id=42, Name="Konya",           Slug="konya",           PlateCode=42 },
        new() { Id=43, Name="KÃ¼tahya",         Slug="kutahya",         PlateCode=43 },
        new() { Id=44, Name="Malatya",         Slug="malatya",         PlateCode=44 },
        new() { Id=45, Name="Manisa",          Slug="manisa",          PlateCode=45 },
        new() { Id=46, Name="KahramanmaraÅŸ",   Slug="kahramanmaras",   PlateCode=46 },
        new() { Id=47, Name="Mardin",          Slug="mardin",          PlateCode=47 },
        new() { Id=48, Name="MuÄŸla",           Slug="mugla",           PlateCode=48 },
        new() { Id=49, Name="MuÅŸ",             Slug="mus",             PlateCode=49 },
        new() { Id=50, Name="NevÅŸehir",        Slug="nevsehir",        PlateCode=50 },
        new() { Id=51, Name="NiÄŸde",           Slug="nigde",           PlateCode=51 },
        new() { Id=52, Name="Ordu",            Slug="ordu",            PlateCode=52 },
        new() { Id=53, Name="Rize",            Slug="rize",            PlateCode=53 },
        new() { Id=54, Name="Sakarya",         Slug="sakarya",         PlateCode=54 },
        new() { Id=55, Name="Samsun",          Slug="samsun",          PlateCode=55 },
        new() { Id=56, Name="Siirt",           Slug="siirt",           PlateCode=56 },
        new() { Id=57, Name="Sinop",           Slug="sinop",           PlateCode=57 },
        new() { Id=58, Name="Sivas",           Slug="sivas",           PlateCode=58 },
        new() { Id=59, Name="TekirdaÄŸ",        Slug="tekirdag",        PlateCode=59 },
        new() { Id=60, Name="Tokat",           Slug="tokat",           PlateCode=60 },
        new() { Id=61, Name="Trabzon",         Slug="trabzon",         PlateCode=61 },
        new() { Id=62, Name="Tunceli",         Slug="tunceli",         PlateCode=62 },
        new() { Id=63, Name="ÅanlÄ±urfa",       Slug="sanliurfa",       PlateCode=63 },
        new() { Id=64, Name="UÅŸak",            Slug="usak",            PlateCode=64 },
        new() { Id=65, Name="Van",             Slug="van",             PlateCode=65 },
        new() { Id=66, Name="Yozgat",          Slug="yozgat",          PlateCode=66 },
        new() { Id=67, Name="Zonguldak",       Slug="zonguldak",       PlateCode=67 },
        new() { Id=68, Name="Aksaray",         Slug="aksaray",         PlateCode=68 },
        new() { Id=69, Name="Bayburt",         Slug="bayburt",         PlateCode=69 },
        new() { Id=70, Name="Karaman",         Slug="karaman",         PlateCode=70 },
        new() { Id=71, Name="KÄ±rÄ±kkale",       Slug="kirikkale",       PlateCode=71 },
        new() { Id=72, Name="Batman",          Slug="batman",          PlateCode=72 },
        new() { Id=73, Name="ÅÄ±rnak",          Slug="sirnak",          PlateCode=73 },
        new() { Id=74, Name="BartÄ±n",          Slug="bartin",          PlateCode=74 },
        new() { Id=75, Name="Ardahan",         Slug="ardahan",         PlateCode=75 },
        new() { Id=76, Name="IÄŸdÄ±r",           Slug="igdir",           PlateCode=76 },
        new() { Id=77, Name="Yalova",          Slug="yalova",          PlateCode=77 },
        new() { Id=78, Name="KarabÃ¼k",         Slug="karabuk",         PlateCode=78 },
        new() { Id=79, Name="Kilis",           Slug="kilis",           PlateCode=79 },
        new() { Id=80, Name="Osmaniye",        Slug="osmaniye",        PlateCode=80 },
        new() { Id=81, Name="DÃ¼zce",           Slug="duzce",           PlateCode=81 },
    };

    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
    // Ä°LÃ‡ELER â€” Her ile ait tam liste
    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
    private static List<District> GetDistricts(List<City> cities)
    {
        var dict = cities.ToDictionary(c => c.PlateCode, c => c.Id);
        var list = new List<District>();
        int id = 1;

        void Add(int plate, params string[] names)
        {
            foreach (var n in names)
                list.Add(new District { Id = id++, CityId = dict[plate], Name = n, Slug = Slugify(n) });
        }

        // 01 Adana
        Add(1, "AladaÄŸ","Ceyhan","Ã‡ukurova","Feke","Ä°mamoÄŸlu","KaraisalÄ±","KarataÅŸ","Kozan","PozantÄ±","Saimbeyli","SarÄ±Ã§am","Seyhan","Tufanbeyli","YumurtalÄ±k","YÃ¼reÄŸir");
        // 02 AdÄ±yaman
        Add(2, "Besni","Ã‡elikhan","Gerger","GÃ¶lbaÅŸÄ±","Kahta","Merkez","Samsat","Sincik","Tut");
        // 03 Afyonkarahisar
        Add(3, "BaÅŸmakÃ§Ä±","Bayat","Bolvadin","Ã‡ay","Ã‡obanlar","DazkÄ±rÄ±","Dinar","EmirdaÄŸ","Evciler","Hocalar","Ä°hsaniye","Ä°scehisar","KÄ±zÄ±lÃ¶ren","Merkez","SandÄ±klÄ±","SinanpaÅŸa","SultandaÄŸÄ±","Åuhut");
        // 04 AÄŸrÄ±
        Add(4, "Diyadin","DoÄŸubayazÄ±t","EleÅŸkirt","Hamur","Merkez","Patnos","TaÅŸlÄ±Ã§ay","Tutak");
        // 05 Amasya
        Add(5, "GÃ¶ynÃ¼cek","GÃ¼mÃ¼ÅŸhacÄ±kÃ¶y","HamamÃ¶zÃ¼","Merkez","Merzifon","Suluova","TaÅŸova");
        // 06 Ankara
        Add(6, "Akyurt","AltÄ±ndaÄŸ","AyaÅŸ","Bala","BeypazarÄ±","Ã‡amlÄ±dere","Ã‡ankaya","Ã‡ubuk","ElmadaÄŸ","Etimesgut","Evren","GÃ¶lbaÅŸÄ±","GÃ¼dÃ¼l","Haymana","Kahramankazan","Kalecik","KeÃ§iÃ¶ren","KÄ±zÄ±lcahamam","Mamak","NallÄ±han","PolatlÄ±","Pursaklar","Sincan","ÅereflikoÃ§hisar","Yenimahalle");
        // 07 Antalya
        Add(7, "Akseki","Aksu","Alanya","Demre","DÃ¶ÅŸemealtÄ±","ElmalÄ±","Finike","GazipaÅŸa","GÃ¼ndoÄŸmuÅŸ","Ä°bradÄ±","KaÅŸ","Kemer","Kepez","KonyaaltÄ±","Korkuteli","Kumluca","Manavgat","MuratpaÅŸa","Serik");
        // 08 Artvin
        Add(8, "ArdanuÃ§","Arhavi","BorÃ§ka","Hopa","KemalpaÅŸa","Merkez","Murgul","ÅavÅŸat","Yusufeli");
        // 09 AydÄ±n
        Add(9, "BozdoÄŸan","Buharkent","Ã‡ine","Didim","Efeler","Germencik","Ä°ncirliova","Karacasu","Karpuzlu","KoÃ§arlÄ±","KÃ¶ÅŸk","KuÅŸadasÄ±","Kuyucak","Nazilli","SÃ¶ke","Sultanhisar","Yenipazar");
        // 10 BalÄ±kesir
        Add(10, "AltÄ±eylÃ¼l","AyvalÄ±k","Balya","BandÄ±rma","BigadiÃ§","Burhaniye","Dursunbey","Edremit","Erdek","GÃ¶meÃ§","GÃ¶nen","Havran","Ä°vrindi","Karesi","Kepsut","Manyas","Marmara","SavaÅŸtepe","SÄ±ndÄ±rgÄ±","Susurluk");
        // 11 Bilecik
        Add(11, "BozÃ¼yÃ¼k","GÃ¶lpazarÄ±","Ä°nhisar","Merkez","Osmaneli","Pazaryeri","SÃ¶ÄŸÃ¼t","Yenipazar");
        // 12 BingÃ¶l
        Add(12, "AdaklÄ±","GenÃ§","KarlÄ±ova","KiÄŸÄ±","Merkez","Solhan","Yayladere","Yedisu");
        // 13 Bitlis
        Add(13, "Adilcevaz","Ahlat","GÃ¼roymak","Hizan","Merkez","Mutki","Tatvan");
        // 14 Bolu
        Add(14, "DÃ¶rtdivan","Gerede","GÃ¶ynÃ¼k","KÄ±brÄ±scÄ±k","Mengen","Merkez","Mudurnu","Seben","YeniÃ§aÄŸa");
        // 15 Burdur
        Add(15, "AÄŸlasun","AltÄ±nyayla","Bucak","Ã‡avdÄ±r","Ã‡eltikÃ§i","GÃ¶lhisar","KaramanlÄ±","Kemer","Merkez","Tefenni","YeÅŸilova");
        // 16 Bursa
        Add(16, "BÃ¼yÃ¼korhan","Gemlik","GÃ¼rsu","HarmancÄ±k","Ä°negÃ¶l","Ä°znik","Karacabey","Keles","Kestel","Mudanya","MustafakemalpaÅŸa","NilÃ¼fer","Orhaneli","Orhangazi","Osmangazi","YeniÅŸehir","YÄ±ldÄ±rÄ±m");
        // 17 Ã‡anakkale
        Add(17, "AyvacÄ±k","BayramiÃ§","Biga","Bozcaada","Ã‡an","Eceabat","Ezine","Gelibolu","GÃ¶kÃ§eada","Lapseki","Merkez","Yenice");
        // 18 Ã‡ankÄ±rÄ±
        Add(18, "Atkaracalar","BayramÃ¶ren","Ã‡erkeÅŸ","Eldivan","Ilgaz","KÄ±zÄ±lÄ±rmak","Korgun","KurÅŸunlu","Merkez","Orta","ÅabanÃ¶zÃ¼","YapraklÄ±");
        // 19 Ã‡orum
        Add(19, "Alaca","Bayat","BoÄŸazkale","Dodurga","Ä°skilip","KargÄ±","LaÃ§in","MecitÃ¶zÃ¼","Merkez","OÄŸuzlar","OrtakÃ¶y","OsmancÄ±k","Sungurlu","UÄŸurludaÄŸ");
        // 20 Denizli
        Add(20, "AcÄ±payam","BabadaÄŸ","Baklan","Bekilli","BeyaÄŸaÃ§","Bozkurt","Buldan","Ã‡al","Ã‡ameli","Ã‡ardak","Ã‡ivril","GÃ¼ney","Honaz","Kale","Merkezefendi","Pamukkale","SaraykÃ¶y","Serinhisar","Tavas");
        // 21 DiyarbakÄ±r
        Add(21, "BaÄŸlar","Bismil","Ã‡ermik","Ã‡Ä±nar","Ã‡Ã¼ngÃ¼ÅŸ","Dicle","EÄŸil","Ergani","Hani","Hazro","KayapÄ±nar","KocakÃ¶y","Kulp","Lice","Silvan","Sur","YeniÅŸehir");
        // 22 Edirne
        Add(22, "Enez","Havsa","Ä°psala","KeÅŸan","LalapaÅŸa","MeriÃ§","Merkez","SÃ¼loÄŸlu","UzunkÃ¶prÃ¼");
        // 23 ElazÄ±ÄŸ
        Add(23, "AÄŸÄ±n","Alacakaya","ArÄ±cak","Baskil","KarakoÃ§an","Keban","KovancÄ±lar","Maden","Merkez","Palu","Sivrice");
        // 24 Erzincan
        Add(24, "Ã‡ayÄ±rlÄ±","Ä°liÃ§","Kemah","Kemaliye","Merkez","Otlukbeli","Refahiye","Tercan","ÃœzÃ¼mlÃ¼");
        // 25 Erzurum
        Add(25, "AÅŸkale","Aziziye","Ã‡at","HÄ±nÄ±s","Horasan","Ä°spir","KaraÃ§oban","KarayazÄ±","KÃ¶prÃ¼kÃ¶y","Merkez","Narman","Oltu","Olur","PalandÃ¶ken","Pasinler","Pazaryolu","Åenkaya","Tekman","Tortum","Uzundere","Yakutiye");
        // 26 EskiÅŸehir
        Add(26, "Alpu","Beylikova","Ã‡ifteler","GÃ¼nyÃ¼zÃ¼","Han","Ä°nÃ¶nÃ¼","Mahmudiye","Mihalgazi","MihalÄ±Ã§Ã§Ä±k","OdunpazarÄ±","SarÄ±cakaya","Seyitgazi","Sivrihisar","TepebaÅŸÄ±");
        // 27 Gaziantep
        Add(27, "Araban","Ä°slahiye","KarkamÄ±ÅŸ","Nizip","NurdaÄŸÄ±","OÄŸuzeli","Åahinbey","Åehitkamil","Yavuzeli");
        // 28 Giresun
        Add(28, "Alucra","Bulancak","Ã‡amoluk","Ã‡anakÃ§Ä±","Dereli","DoÄŸankent","Espiye","Eynesil","GÃ¶rele","GÃ¼ce","KeÅŸap","Merkez","Piraziz","Åebinkarahisar","Tirebolu","YaÄŸlÄ±dere");
        // 29 GÃ¼mÃ¼ÅŸhane
        Add(29, "Kelkit","KÃ¶se","KÃ¼rtÃ¼n","Merkez","Åiran","Torul");
        // 30 Hakkari
        Add(30, "Ã‡ukurca","Derecik","Merkez","Åemdinli","YÃ¼ksekova");
        // 31 Hatay
        Add(31, "AltÄ±nÃ¶zÃ¼","Antakya","Arsuz","Belen","Defne","DÃ¶rtyol","Erzin","Hassa","Ä°skenderun","KÄ±rÄ±khan","Kumlu","Payas","ReyhanlÄ±","SamandaÄŸ","YayladaÄŸÄ±");
        // 32 Isparta
        Add(32, "Aksu","Atabey","EÄŸirdir","Gelendost","GÃ¶nen","KeÃ§iborlu","Merkez","Senirkent","SÃ¼tÃ§Ã¼ler","ÅarkikaraaÄŸaÃ§","Uluborlu","YalvaÃ§","YeniÅŸarbademli");
        // 33 Mersin
        Add(33, "Akdeniz","Anamur","AydÄ±ncÄ±k","BozyazÄ±","Ã‡amlÄ±yayla","Erdemli","GÃ¼lnar","Mezitli","Mut","Silifke","Tarsus","Toroslar","YeniÅŸehir");
        // 34 Ä°stanbul
        Add(34, "Adalar","ArnavutkÃ¶y","AtaÅŸehir","AvcÄ±lar","BaÄŸcÄ±lar","BahÃ§elievler","BakÄ±rkÃ¶y","BaÅŸakÅŸehir","BayrampaÅŸa","BeÅŸiktaÅŸ","Beykoz","BeylikdÃ¼zÃ¼","BeyoÄŸlu","BÃ¼yÃ¼kÃ§ekmece","Ã‡atalca","Ã‡ekmekÃ¶y","Esenler","Esenyurt","EyÃ¼psultan","Fatih","GaziosmanpaÅŸa","GÃ¼ngÃ¶ren","KadÄ±kÃ¶y","KaÄŸÄ±thane","Kartal","KÃ¼Ã§Ã¼kÃ§ekmece","Maltepe","Pendik","Sancaktepe","SarÄ±yer","Silivri","Sultanbeyli","Sultangazi","Åile","ÅiÅŸli","Tuzla","Ãœmraniye","ÃœskÃ¼dar","Zeytinburnu");
        // 35 Ä°zmir
        Add(35, "AliaÄŸa","BalÃ§ova","BayÄ±ndÄ±r","BayraklÄ±","Bergama","BeydaÄŸ","Bornova","Buca","Ã‡eÅŸme","Ã‡iÄŸli","Dikili","FoÃ§a","Gaziemir","GÃ¼zelbahÃ§e","KarabaÄŸlar","Karaburun","KarÅŸÄ±yaka","KemalpaÅŸa","KÄ±nÄ±k","Kiraz","Konak","Menderes","Menemen","NarlÄ±dere","Ã–demiÅŸ","Seferihisar","SelÃ§uk","Tire","TorbalÄ±","Urla");
        // 36 Kars
        Add(36, "Akyaka","ArpaÃ§ay","Digor","KaÄŸÄ±zman","Merkez","SarÄ±kamÄ±ÅŸ","Selim","Susuz");
        // 37 Kastamonu
        Add(37, "Abana","AÄŸlÄ±","AraÃ§","Azdavay","Bozkurt","Cide","Ã‡atalzeytin","Daday","Devrekani","DoÄŸanyurt","HanÃ¶nÃ¼","Ä°hsangazi","Ä°nebolu","KÃ¼re","Merkez","PÄ±narbaÅŸÄ±","Seydiler","Åenpazar","TaÅŸkÃ¶prÃ¼","Tosya");
        // 38 Kayseri
        Add(38, "AkkÄ±ÅŸla","BÃ¼nyan","Develi","Felahiye","HacÄ±lar","Ä°ncesu","Kocasinan","Melikgazi","Ã–zvatan","PÄ±narbaÅŸÄ±","SarÄ±oÄŸlan","SarÄ±z","Talas","Tomarza","YahyalÄ±","YeÅŸilhisar");
        // 39 KÄ±rklareli
        Add(39, "Babaeski","DemirkÃ¶y","KofÃ§az","LÃ¼leburgaz","Merkez","PehlivankÃ¶y","PÄ±narhisar","Vize");
        // 40 KÄ±rÅŸehir
        Add(40, "AkÃ§akent","AkpÄ±nar","Boztepe","Ã‡iÃ§ekdaÄŸÄ±","Kaman","Merkez","Mucur");
        // 41 Kocaeli
        Add(41, "BaÅŸiskele","Ã‡ayÄ±rova","DarÄ±ca","Derince","DilovasÄ±","Gebze","GÃ¶lcÃ¼k","Ä°zmit","KandÄ±ra","KaramÃ¼rsel","Kartepe","KÃ¶rfez");
        // 42 Konya
        Add(42, "AhÄ±rlÄ±","AkÃ¶ren","AkÅŸehir","AltÄ±nekin","BeyÅŸehir","BozkÄ±r","Cihanbeyli","Ã‡eltik","Ã‡umra","Derbent","Derebucak","DoÄŸanhisar","Emirgazi","EreÄŸli","GÃ¼neysÄ±nÄ±r","Hadim","HalkapÄ±nar","HÃ¼yÃ¼k","IlgÄ±n","KadÄ±nhanÄ±","KarapÄ±nar","Karatay","Kulu","Meram","SarayÃ¶nÃ¼","SelÃ§uklu","SeydiÅŸehir","TaÅŸkent","TuzlukÃ§u","YalÄ±hÃ¼yÃ¼k","Yunak");
        // 43 KÃ¼tahya
        Add(43, "AltÄ±ntaÅŸ","Aslanapa","Ã‡avdarhisar","DomaniÃ§","DumlupÄ±nar","Emet","Gediz","HisarcÄ±k","Merkez","Pazarlar","Simav","Åaphane","TavÅŸanlÄ±");
        // 44 Malatya
        Add(44, "AkÃ§adaÄŸ","Arapgir","Arguvan","Battalgazi","Darende","DoÄŸanÅŸehir","DoÄŸanyol","Hekimhan","Kale","Kuluncak","PÃ¼tÃ¼rge","YazÄ±han","YeÅŸilyurt");
        // 45 Manisa
        Add(45, "Ahmetli","Akhisar","AlaÅŸehir","Demirci","GÃ¶lmarmara","GÃ¶rdes","KÄ±rkaÄŸaÃ§","KÃ¶prÃ¼baÅŸÄ±","Kula","Merkez","Salihli","SarÄ±gÃ¶l","SaruhanlÄ±","Selendi","Soma","Åehzadeler","Turgutlu","Yunusemre");
        // 46 KahramanmaraÅŸ
        Add(46, "AfÅŸin","AndÄ±rÄ±n","Ã‡aÄŸlayancerit","DulkadiroÄŸlu","EkinÃ¶zÃ¼","Elbistan","GÃ¶ksun","Nurhak","OnikiÅŸubat","PazarcÄ±k","TÃ¼rkoÄŸlu");
        // 47 Mardin
        Add(47, "Artuklu","DargeÃ§it","Derik","KÄ±zÄ±ltepe","MazÄ±daÄŸÄ±","Midyat","Nusaybin","Ã–merli","Savur","YeÅŸilli");
        // 48 MuÄŸla
        Add(48, "Bodrum","Dalaman","DatÃ§a","Fethiye","KavaklÄ±dere","KÃ¶yceÄŸiz","Marmaris","MenteÅŸe","Milas","Ortaca","Seydikemer","Ula","YataÄŸan");
        // 49 MuÅŸ
        Add(49, "BulanÄ±k","HaskÃ¶y","Korkut","Malazgirt","Merkez","Varto");
        // 50 NevÅŸehir
        Add(50, "AcÄ±gÃ¶l","Avanos","Derinkuyu","GÃ¼lÅŸehir","HacÄ±bektaÅŸ","KozaklÄ±","Merkez","ÃœrgÃ¼p");
        // 51 NiÄŸde
        Add(51, "Altunhisar","Bor","Ã‡amardÄ±","Ã‡iftlik","Merkez","UlukÄ±ÅŸla");
        // 52 Ordu
        Add(52, "AkkuÅŸ","AltÄ±nordu","AybastÄ±","Ã‡amaÅŸ","Ã‡atalpÄ±nar","Ã‡aybaÅŸÄ±","Fatsa","GÃ¶lkÃ¶y","GÃ¼lyalÄ±","GÃ¼rgentepe","Ä°kizce","KabadÃ¼z","KabataÅŸ","Korgan","Kumru","Mesudiye","PerÅŸembe","Ulubey","Ãœnye");
        // 53 Rize
        Add(53, "ArdeÅŸen","Ã‡amlÄ±hemÅŸin","Ã‡ayeli","DerepazarÄ±","FÄ±ndÄ±klÄ±","GÃ¼neysu","HemÅŸin","Ä°kizdere","Ä°yidere","Kalkandere","Merkez","Pazar");
        // 54 Sakarya
        Add(54, "AdapazarÄ±","AkyazÄ±","Arifiye","Erenler","Ferizli","Geyve","Hendek","KarapÃ¼rÃ§ek","Karasu","Kaynarca","Kocaali","MithatpaÅŸa","Pamukova","Sapanca","Serdivan","SÃ¶ÄŸÃ¼tlÃ¼","TaraklÄ±");
        // 55 Samsun
        Add(55, "AlaÃ§am","AsarcÄ±k","Atakum","AyvacÄ±k","Bafra","Canik","Ã‡arÅŸamba","Havza","Ä°lkadÄ±m","Kavak","Ladik","OndokuzmayÄ±s","SalÄ±pazarÄ±","TekkekÃ¶y","Terme","VezirkÃ¶prÃ¼","Yakakent");
        // 56 Siirt
        Add(56, "Baykan","Eruh","Kurtalan","Merkez","Pervari","Åirvan","Tillo");
        // 57 Sinop
        Add(57, "AyancÄ±k","Boyabat","Dikmen","DuraÄŸan","Erfelek","Gerze","Merkez","SaraydÃ¼zÃ¼","TÃ¼rkeli");
        // 58 Sivas
        Add(58, "AkÄ±ncÄ±lar","AltÄ±nyayla","DivriÄŸi","DoÄŸanÅŸar","Gemerek","GÃ¶lova","Hafik","Ä°mranlÄ±","Kangal","Koyulhisar","Merkez","SuÅŸehri","ÅarkÄ±ÅŸla","UlaÅŸ","YÄ±ldÄ±zeli","Zara");
        // 59 TekirdaÄŸ
        Add(59, "Ã‡erkezkÃ¶y","Ã‡orlu","Ergene","Hayrabolu","KapaklÄ±","Malkara","MarmaraereÄŸlisi","MuratlÄ±","Saray","SÃ¼leymanpaÅŸa","ÅarkÃ¶y");
        // 60 Tokat
        Add(60, "Almus","Artova","BaÅŸÃ§iftlik","Erbaa","Merkez","Niksar","Pazar","ReÅŸadiye","Sulusaray","Turhal","YeÅŸilyurt","Zile");
        // 61 Trabzon
        Add(61, "AkÃ§aabat","AraklÄ±","Arsin","BeÅŸikdÃ¼zÃ¼","Ã‡arÅŸÄ±baÅŸÄ±","Ã‡aykara","DernekpazarÄ±","DÃ¼zkÃ¶y","Hayrat","KÃ¶prÃ¼baÅŸÄ±","MaÃ§ka","Of","Ortahisar","SÃ¼rmene","ÅalpazarÄ±","Tonya","VakfÄ±kebir","Yomra");
        // 62 Tunceli
        Add(62, "Ã‡emiÅŸgezek","Hozat","Mazgirt","Merkez","NazÄ±miye","OvacÄ±k","Pertek","PÃ¼lÃ¼mÃ¼r");
        // 63 ÅanlÄ±urfa
        Add(63, "AkÃ§akale","Birecik","Bozova","CeylanpÄ±nar","EyyÃ¼biye","Halfeti","Haliliye","Harran","Hilvan","KarakÃ¶prÃ¼","Siverek","SuruÃ§","ViranÅŸehir");
        // 64 UÅŸak
        Add(64, "Banaz","EÅŸme","KarahallÄ±","Merkez","SivaslÄ±","Ulubey");
        // 65 Van
        Add(65, "BahÃ§esaray","BaÅŸkale","Ã‡aldÄ±ran","Ã‡atak","Edremit","ErciÅŸ","GevaÅŸ","GÃ¼rpÄ±nar","Ä°pekyolu","Muradiye","Ã–zalp","Saray","TuÅŸba");
        // 66 Yozgat
        Add(66, "AkdaÄŸmadeni","AydÄ±ncÄ±k","BoÄŸazlÄ±yan","Ã‡andÄ±r","Ã‡ayÄ±ralan","Ã‡ekerek","KadÄ±ÅŸehri","Merkez","Saraykent","SarÄ±kaya","Åefaatli","Sorgun","YenifakÄ±lÄ±","YerkÃ¶y");
        // 67 Zonguldak
        Add(67, "AlaplÄ±","Ã‡aycuma","Devrek","EreÄŸli","GÃ¶kÃ§ebey","Kilimli","Kozlu","Merkez");
        // 68 Aksaray
        Add(68, "AÄŸaÃ§Ã¶ren","Eskil","GÃ¼laÄŸaÃ§","GÃ¼zelyurt","Merkez","OrtakÃ¶y","SarÄ±yahÅŸi","SultanhanÄ±");
        // 69 Bayburt
        Add(69, "AydÄ±ntepe","DemirÃ¶zÃ¼","Merkez");
        // 70 Karaman
        Add(70, "AyrancÄ±","BaÅŸyayla","Ermenek","KazÄ±mkarabekir","Merkez","SarÄ±veliler");
        // 71 KÄ±rÄ±kkale
        Add(71, "BahÅŸili","BalÄ±ÅŸeyh","Ã‡elebi","Delice","KarakeÃ§ili","Keskin","Merkez","Sulakyurt","YahÅŸihan");
        // 72 Batman
        Add(72, "BeÅŸiri","GercÃ¼ÅŸ","Hasankeyf","Kozluk","Merkez","Sason");
        // 73 ÅÄ±rnak
        Add(73, "BeytÃ¼ÅŸÅŸebap","Cizre","GÃ¼Ã§lÃ¼konak","Ä°dil","Merkez","Silopi","Uludere");
        // 74 BartÄ±n
        Add(74, "Amasra","KurucaÅŸile","Merkez","Ulus");
        // 75 Ardahan
        Add(75, "Ã‡Ä±ldÄ±r","Damal","GÃ¶le","Hanak","Merkez","Posof");
        // 76 IÄŸdÄ±r
        Add(76, "AralÄ±k","Karakoyunlu","Merkez","Tuzluca");
        // 77 Yalova
        Add(77, "AltÄ±nova","Armutlu","Ã‡Ä±narcÄ±k","Ã‡iftlikkÃ¶y","Merkez","Termal");
        // 78 KarabÃ¼k
        Add(78, "Eflani","Eskipazar","Merkez","OvacÄ±k","Safranbolu","Yenice");
        // 79 Kilis
        Add(79, "Elbeyli","Merkez","Musabeyli","Polateli");
        // 80 Osmaniye
        Add(80, "BahÃ§e","DÃ¼ziÃ§i","Hasanbeyli","Kadirli","Merkez","Sumbas","Toprakkale");
        // 81 DÃ¼zce
        Add(81, "AkÃ§akoca","Cumayeri","Ã‡ilimli","GÃ¶lyaka","GÃ¼mÃ¼ÅŸova","KaynaÅŸlÄ±","Merkez","YÄ±ÄŸÄ±lca");

        return list;
    }

    private static Dictionary<int, string[]> GetDistrictsByPlate() => new()
    {
        [1]  = ["AladaÄŸ","Ceyhan","Ã‡ukurova","Feke","Ä°mamoÄŸlu","KaraisalÄ±","KarataÅŸ","Kozan","PozantÄ±","Saimbeyli","SarÄ±Ã§am","Seyhan","Tufanbeyli","YumurtalÄ±k","YÃ¼reÄŸir"],
        [2]  = ["Besni","Ã‡elikhan","Gerger","GÃ¶lbaÅŸÄ±","Kahta","Merkez","Samsat","Sincik","Tut"],
        [3]  = ["BaÅŸmakÃ§Ä±","Bayat","Bolvadin","Ã‡ay","Ã‡obanlar","DazkÄ±rÄ±","Dinar","EmirdaÄŸ","Evciler","Hocalar","Ä°hsaniye","Ä°scehisar","KÄ±zÄ±lÃ¶ren","Merkez","SandÄ±klÄ±","SinanpaÅŸa","SultandaÄŸÄ±","Åuhut"],
        [4]  = ["Diyadin","DoÄŸubayazÄ±t","EleÅŸkirt","Hamur","Merkez","Patnos","TaÅŸlÄ±Ã§ay","Tutak"],
        [5]  = ["GÃ¶ynÃ¼cek","GÃ¼mÃ¼ÅŸhacÄ±kÃ¶y","HamamÃ¶zÃ¼","Merkez","Merzifon","Suluova","TaÅŸova"],
        [6]  = ["Akyurt","AltÄ±ndaÄŸ","AyaÅŸ","Bala","BeypazarÄ±","Ã‡amlÄ±dere","Ã‡ankaya","Ã‡ubuk","ElmadaÄŸ","Etimesgut","Evren","GÃ¶lbaÅŸÄ±","GÃ¼dÃ¼l","Haymana","Kahramankazan","Kalecik","KeÃ§iÃ¶ren","KÄ±zÄ±lcahamam","Mamak","NallÄ±han","PolatlÄ±","Pursaklar","Sincan","ÅereflikoÃ§hisar","Yenimahalle"],
        [7]  = ["Akseki","Aksu","Alanya","Demre","DÃ¶ÅŸemealtÄ±","ElmalÄ±","Finike","GazipaÅŸa","GÃ¼ndoÄŸmuÅŸ","Ä°bradÄ±","KaÅŸ","Kemer","Kepez","KonyaaltÄ±","Korkuteli","Kumluca","Manavgat","MuratpaÅŸa","Serik"],
        [8]  = ["ArdanuÃ§","Arhavi","BorÃ§ka","Hopa","KemalpaÅŸa","Merkez","Murgul","ÅavÅŸat","Yusufeli"],
        [9]  = ["BozdoÄŸan","Buharkent","Ã‡ine","Didim","Efeler","Germencik","Ä°ncirliova","Karacasu","Karpuzlu","KoÃ§arlÄ±","KÃ¶ÅŸk","KuÅŸadasÄ±","Kuyucak","Nazilli","SÃ¶ke","Sultanhisar","Yenipazar"],
        [10] = ["AltÄ±eylÃ¼l","AyvalÄ±k","Balya","BandÄ±rma","BigadiÃ§","Burhaniye","Dursunbey","Edremit","Erdek","GÃ¶meÃ§","GÃ¶nen","Havran","Ä°vrindi","Karesi","Kepsut","Manyas","Marmara","SavaÅŸtepe","SÄ±ndÄ±rgÄ±","Susurluk"],
        [11] = ["BozÃ¼yÃ¼k","GÃ¶lpazarÄ±","Ä°nhisar","Merkez","Osmaneli","Pazaryeri","SÃ¶ÄŸÃ¼t","Yenipazar"],
        [12] = ["AdaklÄ±","GenÃ§","KarlÄ±ova","KiÄŸÄ±","Merkez","Solhan","Yayladere","Yedisu"],
        [13] = ["Adilcevaz","Ahlat","GÃ¼roymak","Hizan","Merkez","Mutki","Tatvan"],
        [14] = ["DÃ¶rtdivan","Gerede","GÃ¶ynÃ¼k","KÄ±brÄ±scÄ±k","Mengen","Merkez","Mudurnu","Seben","YeniÃ§aÄŸa"],
        [15] = ["AÄŸlasun","AltÄ±nyayla","Bucak","Ã‡avdÄ±r","Ã‡eltikÃ§i","GÃ¶lhisar","KaramanlÄ±","Kemer","Merkez","Tefenni","YeÅŸilova"],
        [16] = ["BÃ¼yÃ¼korhan","Gemlik","GÃ¼rsu","HarmancÄ±k","Ä°negÃ¶l","Ä°znik","Karacabey","Keles","Kestel","Mudanya","MustafakemalpaÅŸa","NilÃ¼fer","Orhaneli","Orhangazi","Osmangazi","YeniÅŸehir","YÄ±ldÄ±rÄ±m"],
        [17] = ["AyvacÄ±k","BayramiÃ§","Biga","Bozcaada","Ã‡an","Eceabat","Ezine","Gelibolu","GÃ¶kÃ§eada","Lapseki","Merkez","Yenice"],
        [18] = ["Atkaracalar","BayramÃ¶ren","Ã‡erkeÅŸ","Eldivan","Ilgaz","KÄ±zÄ±lÄ±rmak","Korgun","KurÅŸunlu","Merkez","Orta","ÅabanÃ¶zÃ¼","YapraklÄ±"],
        [19] = ["Alaca","Bayat","BoÄŸazkale","Dodurga","Ä°skilip","KargÄ±","LaÃ§in","MecitÃ¶zÃ¼","Merkez","OÄŸuzlar","OrtakÃ¶y","OsmancÄ±k","Sungurlu","UÄŸurludaÄŸ"],
        [20] = ["AcÄ±payam","BabadaÄŸ","Baklan","Bekilli","BeyaÄŸaÃ§","Bozkurt","Buldan","Ã‡al","Ã‡ameli","Ã‡ardak","Ã‡ivril","GÃ¼ney","Honaz","Kale","Merkezefendi","Pamukkale","SaraykÃ¶y","Serinhisar","Tavas"],
        [21] = ["BaÄŸlar","Bismil","Ã‡ermik","Ã‡Ä±nar","Ã‡Ã¼ngÃ¼ÅŸ","Dicle","EÄŸil","Ergani","Hani","Hazro","KayapÄ±nar","KocakÃ¶y","Kulp","Lice","Silvan","Sur","YeniÅŸehir"],
        [22] = ["Enez","Havsa","Ä°psala","KeÅŸan","LalapaÅŸa","MeriÃ§","Merkez","SÃ¼loÄŸlu","UzunkÃ¶prÃ¼"],
        [23] = ["AÄŸÄ±n","Alacakaya","ArÄ±cak","Baskil","KarakoÃ§an","Keban","KovancÄ±lar","Maden","Merkez","Palu","Sivrice"],
        [24] = ["Ã‡ayÄ±rlÄ±","Ä°liÃ§","Kemah","Kemaliye","Merkez","Otlukbeli","Refahiye","Tercan","ÃœzÃ¼mlÃ¼"],
        [25] = ["AÅŸkale","Aziziye","Ã‡at","HÄ±nÄ±s","Horasan","Ä°spir","KaraÃ§oban","KarayazÄ±","KÃ¶prÃ¼kÃ¶y","Merkez","Narman","Oltu","Olur","PalandÃ¶ken","Pasinler","Pazaryolu","Åenkaya","Tekman","Tortum","Uzundere","Yakutiye"],
        [26] = ["Alpu","Beylikova","Ã‡ifteler","GÃ¼nyÃ¼zÃ¼","Han","Ä°nÃ¶nÃ¼","Mahmudiye","Mihalgazi","MihalÄ±Ã§Ã§Ä±k","OdunpazarÄ±","SarÄ±cakaya","Seyitgazi","Sivrihisar","TepebaÅŸÄ±"],
        [27] = ["Araban","Ä°slahiye","KarkamÄ±ÅŸ","Nizip","NurdaÄŸÄ±","OÄŸuzeli","Åahinbey","Åehitkamil","Yavuzeli"],
        [28] = ["Alucra","Bulancak","Ã‡amoluk","Ã‡anakÃ§Ä±","Dereli","DoÄŸankent","Espiye","Eynesil","GÃ¶rele","GÃ¼ce","KeÅŸap","Merkez","Piraziz","Åebinkarahisar","Tirebolu","YaÄŸlÄ±dere"],
        [29] = ["Kelkit","KÃ¶se","KÃ¼rtÃ¼n","Merkez","Åiran","Torul"],
        [30] = ["Ã‡ukurca","Derecik","Merkez","Åemdinli","YÃ¼ksekova"],
        [31] = ["AltÄ±nÃ¶zÃ¼","Antakya","Arsuz","Belen","Defne","DÃ¶rtyol","Erzin","Hassa","Ä°skenderun","KÄ±rÄ±khan","Kumlu","Payas","ReyhanlÄ±","SamandaÄŸ","YayladaÄŸÄ±"],
        [32] = ["Aksu","Atabey","EÄŸirdir","Gelendost","GÃ¶nen","KeÃ§iborlu","Merkez","Senirkent","SÃ¼tÃ§Ã¼ler","ÅarkikaraaÄŸaÃ§","Uluborlu","YalvaÃ§","YeniÅŸarbademli"],
        [33] = ["Akdeniz","Anamur","AydÄ±ncÄ±k","BozyazÄ±","Ã‡amlÄ±yayla","Erdemli","GÃ¼lnar","Mezitli","Mut","Silifke","Tarsus","Toroslar","YeniÅŸehir"],
        [34] = ["Adalar","ArnavutkÃ¶y","AtaÅŸehir","AvcÄ±lar","BaÄŸcÄ±lar","BahÃ§elievler","BakÄ±rkÃ¶y","BaÅŸakÅŸehir","BayrampaÅŸa","BeÅŸiktaÅŸ","Beykoz","BeylikdÃ¼zÃ¼","BeyoÄŸlu","BÃ¼yÃ¼kÃ§ekmece","Ã‡atalca","Ã‡ekmekÃ¶y","Esenler","Esenyurt","EyÃ¼psultan","Fatih","GaziosmanpaÅŸa","GÃ¼ngÃ¶ren","KadÄ±kÃ¶y","KaÄŸÄ±thane","Kartal","KÃ¼Ã§Ã¼kÃ§ekmece","Maltepe","Pendik","Sancaktepe","SarÄ±yer","Silivri","Sultanbeyli","Sultangazi","Åile","ÅiÅŸli","Tuzla","Ãœmraniye","ÃœskÃ¼dar","Zeytinburnu"],
        [35] = ["AliaÄŸa","BalÃ§ova","BayÄ±ndÄ±r","BayraklÄ±","Bergama","BeydaÄŸ","Bornova","Buca","Ã‡eÅŸme","Ã‡iÄŸli","Dikili","FoÃ§a","Gaziemir","GÃ¼zelbahÃ§e","KarabaÄŸlar","Karaburun","KarÅŸÄ±yaka","KemalpaÅŸa","KÄ±nÄ±k","Kiraz","Konak","Menderes","Menemen","NarlÄ±dere","Ã–demiÅŸ","Seferihisar","SelÃ§uk","Tire","TorbalÄ±","Urla"],
        [36] = ["Akyaka","ArpaÃ§ay","Digor","KaÄŸÄ±zman","Merkez","SarÄ±kamÄ±ÅŸ","Selim","Susuz"],
        [37] = ["Abana","AÄŸlÄ±","AraÃ§","Azdavay","Bozkurt","Cide","Ã‡atalzeytin","Daday","Devrekani","DoÄŸanyurt","HanÃ¶nÃ¼","Ä°hsangazi","Ä°nebolu","KÃ¼re","Merkez","PÄ±narbaÅŸÄ±","Seydiler","Åenpazar","TaÅŸkÃ¶prÃ¼","Tosya"],
        [38] = ["AkkÄ±ÅŸla","BÃ¼nyan","Develi","Felahiye","HacÄ±lar","Ä°ncesu","Kocasinan","Melikgazi","Ã–zvatan","PÄ±narbaÅŸÄ±","SarÄ±oÄŸlan","SarÄ±z","Talas","Tomarza","YahyalÄ±","YeÅŸilhisar"],
        [39] = ["Babaeski","DemirkÃ¶y","KofÃ§az","LÃ¼leburgaz","Merkez","PehlivankÃ¶y","PÄ±narhisar","Vize"],
        [40] = ["AkÃ§akent","AkpÄ±nar","Boztepe","Ã‡iÃ§ekdaÄŸÄ±","Kaman","Merkez","Mucur"],
        [41] = ["BaÅŸiskele","Ã‡ayÄ±rova","DarÄ±ca","Derince","DilovasÄ±","Gebze","GÃ¶lcÃ¼k","Ä°zmit","KandÄ±ra","KaramÃ¼rsel","Kartepe","KÃ¶rfez"],
        [42] = ["AhÄ±rlÄ±","AkÃ¶ren","AkÅŸehir","AltÄ±nekin","BeyÅŸehir","BozkÄ±r","Cihanbeyli","Ã‡eltik","Ã‡umra","Derbent","Derebucak","DoÄŸanhisar","Emirgazi","EreÄŸli","GÃ¼neysÄ±nÄ±r","Hadim","HalkapÄ±nar","HÃ¼yÃ¼k","IlgÄ±n","KadÄ±nhanÄ±","KarapÄ±nar","Karatay","Kulu","Meram","SarayÃ¶nÃ¼","SelÃ§uklu","SeydiÅŸehir","TaÅŸkent","TuzlukÃ§u","YalÄ±hÃ¼yÃ¼k","Yunak"],
        [43] = ["AltÄ±ntaÅŸ","Aslanapa","Ã‡avdarhisar","DomaniÃ§","DumlupÄ±nar","Emet","Gediz","HisarcÄ±k","Merkez","Pazarlar","Simav","Åaphane","TavÅŸanlÄ±"],
        [44] = ["AkÃ§adaÄŸ","Arapgir","Arguvan","Battalgazi","Darende","DoÄŸanÅŸehir","DoÄŸanyol","Hekimhan","Kale","Kuluncak","PÃ¼tÃ¼rge","YazÄ±han","YeÅŸilyurt"],
        [45] = ["Ahmetli","Akhisar","AlaÅŸehir","Demirci","GÃ¶lmarmara","GÃ¶rdes","KÄ±rkaÄŸaÃ§","KÃ¶prÃ¼baÅŸÄ±","Kula","Merkez","Salihli","SarÄ±gÃ¶l","SaruhanlÄ±","Selendi","Soma","Åehzadeler","Turgutlu","Yunusemre"],
        [46] = ["AfÅŸin","AndÄ±rÄ±n","Ã‡aÄŸlayancerit","DulkadiroÄŸlu","EkinÃ¶zÃ¼","Elbistan","GÃ¶ksun","Nurhak","OnikiÅŸubat","PazarcÄ±k","TÃ¼rkoÄŸlu"],
        [47] = ["Artuklu","DargeÃ§it","Derik","KÄ±zÄ±ltepe","MazÄ±daÄŸÄ±","Midyat","Nusaybin","Ã–merli","Savur","YeÅŸilli"],
        [48] = ["Bodrum","Dalaman","DatÃ§a","Fethiye","KavaklÄ±dere","KÃ¶yceÄŸiz","Marmaris","MenteÅŸe","Milas","Ortaca","Seydikemer","Ula","YataÄŸan"],
        [49] = ["BulanÄ±k","HaskÃ¶y","Korkut","Malazgirt","Merkez","Varto"],
        [50] = ["AcÄ±gÃ¶l","Avanos","Derinkuyu","GÃ¼lÅŸehir","HacÄ±bektaÅŸ","KozaklÄ±","Merkez","ÃœrgÃ¼p"],
        [51] = ["Altunhisar","Bor","Ã‡amardÄ±","Ã‡iftlik","Merkez","UlukÄ±ÅŸla"],
        [52] = ["AkkuÅŸ","AltÄ±nordu","AybastÄ±","Ã‡amaÅŸ","Ã‡atalpÄ±nar","Ã‡aybaÅŸÄ±","Fatsa","GÃ¶lkÃ¶y","GÃ¼lyalÄ±","GÃ¼rgentepe","Ä°kizce","KabadÃ¼z","KabataÅŸ","Korgan","Kumru","Mesudiye","PerÅŸembe","Ulubey","Ãœnye"],
        [53] = ["ArdeÅŸen","Ã‡amlÄ±hemÅŸin","Ã‡ayeli","DerepazarÄ±","FÄ±ndÄ±klÄ±","GÃ¼neysu","HemÅŸin","Ä°kizdere","Ä°yidere","Kalkandere","Merkez","Pazar"],
        [54] = ["AdapazarÄ±","AkyazÄ±","Arifiye","Erenler","Ferizli","Geyve","Hendek","KarapÃ¼rÃ§ek","Karasu","Kaynarca","Kocaali","MithatpaÅŸa","Pamukova","Sapanca","Serdivan","SÃ¶ÄŸÃ¼tlÃ¼","TaraklÄ±"],
        [55] = ["AlaÃ§am","AsarcÄ±k","Atakum","AyvacÄ±k","Bafra","Canik","Ã‡arÅŸamba","Havza","Ä°lkadÄ±m","Kavak","Ladik","OndokuzmayÄ±s","SalÄ±pazarÄ±","TekkekÃ¶y","Terme","VezirkÃ¶prÃ¼","Yakakent"],
        [56] = ["Baykan","Eruh","Kurtalan","Merkez","Pervari","Åirvan","Tillo"],
        [57] = ["AyancÄ±k","Boyabat","Dikmen","DuraÄŸan","Erfelek","Gerze","Merkez","SaraydÃ¼zÃ¼","TÃ¼rkeli"],
        [58] = ["AkÄ±ncÄ±lar","AltÄ±nyayla","DivriÄŸi","DoÄŸanÅŸar","Gemerek","GÃ¶lova","Hafik","Ä°mranlÄ±","Kangal","Koyulhisar","Merkez","SuÅŸehri","ÅarkÄ±ÅŸla","UlaÅŸ","YÄ±ldÄ±zeli","Zara"],
        [59] = ["Ã‡erkezkÃ¶y","Ã‡orlu","Ergene","Hayrabolu","KapaklÄ±","Malkara","MarmaraereÄŸlisi","MuratlÄ±","Saray","SÃ¼leymanpaÅŸa","ÅarkÃ¶y"],
        [60] = ["Almus","Artova","BaÅŸÃ§iftlik","Erbaa","Merkez","Niksar","Pazar","ReÅŸadiye","Sulusaray","Turhal","YeÅŸilyurt","Zile"],
        [61] = ["AkÃ§aabat","AraklÄ±","Arsin","BeÅŸikdÃ¼zÃ¼","Ã‡arÅŸÄ±baÅŸÄ±","Ã‡aykara","DernekpazarÄ±","DÃ¼zkÃ¶y","Hayrat","KÃ¶prÃ¼baÅŸÄ±","MaÃ§ka","Of","Ortahisar","SÃ¼rmene","ÅalpazarÄ±","Tonya","VakfÄ±kebir","Yomra"],
        [62] = ["Ã‡emiÅŸgezek","Hozat","Mazgirt","Merkez","NazÄ±miye","OvacÄ±k","Pertek","PÃ¼lÃ¼mÃ¼r"],
        [63] = ["AkÃ§akale","Birecik","Bozova","CeylanpÄ±nar","EyyÃ¼biye","Halfeti","Haliliye","Harran","Hilvan","KarakÃ¶prÃ¼","Siverek","SuruÃ§","ViranÅŸehir"],
        [64] = ["Banaz","EÅŸme","KarahallÄ±","Merkez","SivaslÄ±","Ulubey"],
        [65] = ["BahÃ§esaray","BaÅŸkale","Ã‡aldÄ±ran","Ã‡atak","Edremit","ErciÅŸ","GevaÅŸ","GÃ¼rpÄ±nar","Ä°pekyolu","Muradiye","Ã–zalp","Saray","TuÅŸba"],
        [66] = ["AkdaÄŸmadeni","AydÄ±ncÄ±k","BoÄŸazlÄ±yan","Ã‡andÄ±r","Ã‡ayÄ±ralan","Ã‡ekerek","KadÄ±ÅŸehri","Merkez","Saraykent","SarÄ±kaya","Åefaatli","Sorgun","YenifakÄ±lÄ±","YerkÃ¶y"],
        [67] = ["AlaplÄ±","Ã‡aycuma","Devrek","EreÄŸli","GÃ¶kÃ§ebey","Kilimli","Kozlu","Merkez"],
        [68] = ["AÄŸaÃ§Ã¶ren","Eskil","GÃ¼laÄŸaÃ§","GÃ¼zelyurt","Merkez","OrtakÃ¶y","SarÄ±yahÅŸi","SultanhanÄ±"],
        [69] = ["AydÄ±ntepe","DemirÃ¶zÃ¼","Merkez"],
        [70] = ["AyrancÄ±","BaÅŸyayla","Ermenek","KazÄ±mkarabekir","Merkez","SarÄ±veliler"],
        [71] = ["BahÅŸili","BalÄ±ÅŸeyh","Ã‡elebi","Delice","KarakeÃ§ili","Keskin","Merkez","Sulakyurt","YahÅŸihan"],
        [72] = ["BeÅŸiri","GercÃ¼ÅŸ","Hasankeyf","Kozluk","Merkez","Sason"],
        [73] = ["BeytÃ¼ÅŸÅŸebap","Cizre","GÃ¼Ã§lÃ¼konak","Ä°dil","Merkez","Silopi","Uludere"],
        [74] = ["Amasra","KurucaÅŸile","Merkez","Ulus"],
        [75] = ["Ã‡Ä±ldÄ±r","Damal","GÃ¶le","Hanak","Merkez","Posof"],
        [76] = ["AralÄ±k","Karakoyunlu","Merkez","Tuzluca"],
        [77] = ["AltÄ±nova","Armutlu","Ã‡Ä±narcÄ±k","Ã‡iftlikkÃ¶y","Merkez","Termal"],
        [78] = ["Eflani","Eskipazar","Merkez","OvacÄ±k","Safranbolu","Yenice"],
        [79] = ["Elbeyli","Merkez","Musabeyli","Polateli"],
        [80] = ["BahÃ§e","DÃ¼ziÃ§i","Hasanbeyli","Kadirli","Merkez","Sumbas","Toprakkale"],
        [81] = ["AkÃ§akoca","Cumayeri","Ã‡ilimli","GÃ¶lyaka","GÃ¼mÃ¼ÅŸova","KaynaÅŸlÄ±","Merkez","YÄ±ÄŸÄ±lca"],
    };

    private static string Slugify(string name) =>
        name.ToLowerInvariant()
            .Replace("ÅŸ","s").Replace("ÄŸ","g").Replace("Ã¼","u")
            .Replace("Ã¶","o").Replace("Ä±","i").Replace("Ã§","c")
            .Replace("Ä°","i").Replace("Å","s").Replace("Ä","g")
            .Replace(" ","-").Replace("'","");


    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
    // BRANÅLAR â€” KapsamlÄ± liste, kategorili
    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
    private static List<Branch> GetBranches()
    {
        int id = 1;
        int order = 1;
        var list = new List<Branch>();

        void Add(string name, string category, bool popular = false) =>
            list.Add(new Branch { Id = id++, Name = name, Slug = Slugify(name), Category = category, IsPopular = popular, DisplayOrder = order++ });

        // Akademik â€” Temel Dersler
        Add("Matematik",           "Akademik", popular: true);
        Add("Fizik",               "Akademik", popular: true);
        Add("Kimya",               "Akademik", popular: true);
        Add("Biyoloji",            "Akademik", popular: true);
        Add("TÃ¼rkÃ§e / Edebiyat",   "Akademik", popular: true);
        Add("Tarih",               "Akademik");
        Add("CoÄŸrafya",            "Akademik");
        Add("Felsefe",             "Akademik");
        Add("Din KÃ¼ltÃ¼rÃ¼",         "Akademik");
        Add("Ä°ngilizce",           "Dil",      popular: true);
        Add("Almanca",             "Dil",      popular: true);
        Add("FransÄ±zca",           "Dil");
        Add("Ä°spanyolca",          "Dil");
        Add("Ä°talyanca",           "Dil");
        Add("ArapÃ§a",              "Dil");
        Add("RusÃ§a",               "Dil");
        Add("Japonca",             "Dil");
        Add("Ã‡ince",               "Dil");
        Add("Korece",              "Dil");

        // SÄ±nav HazÄ±rlÄ±k
        Add("YKS / TYT Matematik", "SÄ±nav HazÄ±rlÄ±k", popular: true);
        Add("YKS / AYT Fizik",     "SÄ±nav HazÄ±rlÄ±k", popular: true);
        Add("YKS / AYT Kimya",     "SÄ±nav HazÄ±rlÄ±k", popular: true);
        Add("YKS / AYT Biyoloji",  "SÄ±nav HazÄ±rlÄ±k");
        Add("YKS / AYT Edebiyat",  "SÄ±nav HazÄ±rlÄ±k");
        Add("YKS / AYT Tarih",     "SÄ±nav HazÄ±rlÄ±k");
        Add("YKS / AYT CoÄŸrafya",  "SÄ±nav HazÄ±rlÄ±k");
        Add("LGS HazÄ±rlÄ±k",        "SÄ±nav HazÄ±rlÄ±k", popular: true);
        Add("KPSS",                "SÄ±nav HazÄ±rlÄ±k", popular: true);
        Add("ALES",                "SÄ±nav HazÄ±rlÄ±k");
        Add("YDS / YÃ–KDÄ°L",        "SÄ±nav HazÄ±rlÄ±k");
        Add("DGS",                 "SÄ±nav HazÄ±rlÄ±k");
        Add("Ã–SYM SÄ±navlarÄ±",      "SÄ±nav HazÄ±rlÄ±k");

        // Teknoloji & YazÄ±lÄ±m
        Add("Python",              "YazÄ±lÄ±m", popular: true);
        Add("JavaScript",          "YazÄ±lÄ±m", popular: true);
        Add("Java",                "YazÄ±lÄ±m", popular: true);
        Add("C# / .NET",           "YazÄ±lÄ±m", popular: true);
        Add("C / C++",             "YazÄ±lÄ±m");
        Add("PHP",                 "YazÄ±lÄ±m");
        Add("Swift / iOS",         "YazÄ±lÄ±m");
        Add("Kotlin / Android",    "YazÄ±lÄ±m");
        Add("React",               "YazÄ±lÄ±m", popular: true);
        Add("Vue.js",              "YazÄ±lÄ±m");
        Add("Angular",             "YazÄ±lÄ±m");
        Add("Node.js",             "YazÄ±lÄ±m");
        Add("SQL / VeritabanÄ±",    "YazÄ±lÄ±m");
        Add("Siber GÃ¼venlik",      "YazÄ±lÄ±m");
        Add("Veri Bilimi",         "YazÄ±lÄ±m", popular: true);
        Add("Yapay Zeka / ML",     "YazÄ±lÄ±m", popular: true);
        Add("Unity / Oyun GeliÅŸtirme", "YazÄ±lÄ±m");
        Add("Web TasarÄ±m",         "YazÄ±lÄ±m");

        // MÃ¼zik
        Add("Piyano",              "MÃ¼zik", popular: true);
        Add("Gitar (Klasik)",      "MÃ¼zik", popular: true);
        Add("Gitar (Elektro/Akustik)", "MÃ¼zik");
        Add("Keman",               "MÃ¼zik");
        Add("Viyola",              "MÃ¼zik");
        Add("Ã‡ello",               "MÃ¼zik");
        Add("FlÃ¼t",                "MÃ¼zik");
        Add("Klarnet",             "MÃ¼zik");
        Add("Saksofon",            "MÃ¼zik");
        Add("Davul / PerkÃ¼syon",   "MÃ¼zik");
        Add("BaÄŸlama / Saz",       "MÃ¼zik", popular: true);
        Add("Ud",                  "MÃ¼zik");
        Add("Åan / Vokal",         "MÃ¼zik");

        // Sanat & TasarÄ±m
        Add("Resim",               "Sanat");
        Add("YaÄŸlÄ± Boya",          "Sanat");
        Add("Suluboya",            "Sanat");
        Add("Heykel",              "Sanat");
        Add("Grafik TasarÄ±m",      "Sanat", popular: true);
        Add("FotoÄŸrafÃ§Ä±lÄ±k",       "Sanat");
        Add("Video DÃ¼zenleme",     "Sanat");

        // Spor & Aktivite
        Add("YÃ¼zme",               "Spor", popular: true);
        Add("Tenis",               "Spor");
        Add("SatranÃ§",             "Spor", popular: true);
        Add("Yoga",                "Spor");
        Add("Pilates",             "Spor");
        Add("Dans (Salsa/Tango)",  "Spor");
        Add("Bale",                "Spor");
        Add("Jimnastik",           "Spor");
        Add("Futbol AntrenÃ¶rlÃ¼ÄŸÃ¼", "Spor");
        Add("Basketbol",           "Spor");
        Add("Voleybol",            "Spor");
        Add("DÃ¶vÃ¼ÅŸ SanatlarÄ±",     "Spor");

        // DiÄŸer
        Add("Muhasebe",            "DiÄŸer");
        Add("GiriÅŸimcilik",        "DiÄŸer");
        Add("Diksiyon / Sunum",    "DiÄŸer");
        Add("HÄ±z Okuma",           "DiÄŸer");
        Add("Zihin HaritasÄ±",      "DiÄŸer");

        return list;
    }
}
