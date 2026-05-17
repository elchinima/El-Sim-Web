const languageStorageKey = "elsim-language";

const translations = {
    ru: {
        "Pass": "Pass",
        "Plans": "Тарифы",
        "Global": "Глобал",
        "Activation": "Активация",
        "WI-FI": "WI-FI",
        "Favorite": "\u041b\u044e\u0431\u0438\u043c\u044b\u0439",
        "Support": "Поддержка",
        "Login": "Войти",
        "Home": "Главная",
        "El-Sim eSIM": "El-Sim eSIM",
        "Instant mobile access": "Мгновенный мобильный доступ",
        "Activate your digital SIM with a QR code and stay connected without plastic cards.": "Активируйте цифровую SIM по QR-коду и оставайтесь на связи без пластиковых карт.",
        "Fast setup": "Быстрая настройка",
        "Scan, install, go online": "Сканируйте, установите, подключайтесь",
        "A smooth activation flow built for modern phones and everyday travel.": "Удобная активация для современных телефонов и ежедневных поездок.",
        "Coverage": "Покрытие",
        "Made for Azerbaijan": "Создано для Азербайджана",
        "Reliable digital connectivity across the city, regions, and the road between them.": "Надежная цифровая связь в городе, регионах и в дороге между ними.",
        "digital SIM for Azerbaijan": "цифровая SIM для Азербайджана",
        "Mobile connection without a plastic SIM card": "Мобильная связь без пластиковой SIM-карты",
        "Choose a plan, get your QR code, and activate El-Sim on your phone in just a few minutes.": "Выберите тариф, получите QR-код и активируйте El-Sim на телефоне за несколько минут.",
        "Choose a plan": "Выбрать тариф",
        "How it works": "Как это работает",
        "activation": "активация",
        "Three simple steps": "Три простых шага",
        "Choose a plan": "Выберите тариф",
        "Pick the data package you need and pay online with a convenient method.": "Выберите нужный пакет данных и оплатите онлайн удобным способом.",
        "Get your QR code": "Получите QR-код",
        "Your activation code arrives by email right after purchase.": "Код активации придет на email сразу после покупки.",
        "Connect": "Подключитесь",
        "Scan the QR code in your phone settings and stay online.": "Отсканируйте QR-код в настройках телефона и оставайтесь онлайн.",
        "plans": "тарифы",
        "Simple packages": "Простые пакеты",
        "Start": "Start",
        "5 GB of mobile data": "5 GB мобильного интернета",
        "Choose": "Выбрать",
        "Smart": "Smart",
        "10 GB, calls and SMS": "10 GB, звонки и SMS",
        "Max": "Max",
        "Unlimited mobile data": "Безлимитный мобильный интернет",
        "wi-fi fallback": "резервный wi-fi",
        "Stay online between networks": "Оставайтесь онлайн между сетями",
        "Use El-Sim support to find nearby Wi-Fi and finish activation when mobile coverage is weak.": "Используйте поддержку El-Sim, чтобы найти Wi-Fi рядом и завершить активацию при слабом мобильном покрытии.",
        "Get help": "Получить помощь",
        "support": "поддержка",
        "We can help you connect": "Мы поможем подключиться",
        "Message us if you need device checks, QR code help, or plan recommendations.": "Напишите нам, если нужна проверка устройства, помощь с QR-кодом или рекомендация тарифа.",
        "Contact us": "Связаться",
        "Baku, Azerbaijan": "Баку, Азербайджан",
        "secure access": "безопасный доступ",
        "Manage your El-Sim profile": "Управляйте профилем El-Sim",
        "Sign in with your FIN and password, or create a new profile before choosing and activating your digital SIM.": "Войдите по FIN и паролю или создайте новый профиль перед выбором и активацией цифровой SIM.",
        "Fast activation": "Быстрая активация",
        "Plan history": "История тарифов",
        "Support access": "Доступ к поддержке",
        "Register": "Регистрация",
        "Name": "Имя",
        "Password": "Пароль",
        "Exactly 7 English letters or digits.": "Ровно 7 английских букв или цифр.",
        "Minimum 8 characters.": "Минимум 8 символов.",
        "Create account": "Создать аккаунт",
        "membership": "подписка",
        "Unlock monthly El-Sim benefits, bonus data opportunities, partner rewards, and stronger network access.": "Откройте ежемесячные преимущества El-Sim, бонусный интернет, награды от партнеров и улучшенный доступ к сети.",
        "4.00 AZN | 30 days": "4.00 AZN | 30 дней",
        "7.00 AZN | 30 days": "7.00 AZN | 30 дней",
        "11.00 AZN | 30 days": "11.00 AZN | 30 дней",
        "Up to 50 free calls": "До 50 бесплатных звонков",
        "Bonuses from our partners": "Бонусы от наших партнеров",
        "Opportunity to earn up to 5GB": "Возможность получить до 5GB",
        "Connection to next-generation 5G internet": "Подключение к интернету 5G нового поколения",
        "Up to 50% ad blocking": "Блокировка рекламы до 50%",
        "5G+ internet connection": "Подключение к интернету 5G+",
        "Up to 100 free calls": "До 100 бесплатных звонков",
        "Opportunity to earn up to 10GB": "Возможность получить до 10GB",
        "Higher bonuses from partners": "Повышенные бонусы от партнеров",
        "Up to 70% ad blocking": "Блокировка рекламы до 70%",
        "VIP customer service": "VIP обслуживание клиентов",
        "Up to 150 free calls": "До 150 бесплатных звонков",
        "Opportunity to earn up to 15GB": "Возможность получить до 15GB",
        "Ultra bonuses from partners": "Ultra бонусы от партнеров",
        "Ability to change the app icon": "Возможность менять иконку приложения",
        "Up to 90% ad blocking": "Блокировка рекламы до 90%",
        "Network access anywhere in the Republic of Azerbaijan": "Доступ к сети в любой точке Азербайджанской Республики",
        "tariffs": "тарифы",
        "Choose a monthly package with local minutes, local SMS, and mobile internet for everyday connection.": "Выберите месячный пакет с местными минутами, SMS и мобильным интернетом для ежедневной связи.",
        "eSIM price": "Цена eSIM",
        "50% off all plans for the first month": "Скидка 50% на все тарифы в первый месяц",
        "30 days": "30 дней",
        "7 days": "7 дней",
        "30 free domestic minutes": "30 бесплатных минут внутри страны",
        "30 free domestic SMS": "30 бесплатных SMS внутри страны",
        "50 free domestic minutes": "50 бесплатных минут внутри страны",
        "50 free domestic SMS": "50 бесплатных SMS внутри страны",
        "150 free domestic minutes": "150 бесплатных минут внутри страны",
        "150 free domestic SMS": "150 бесплатных SMS внутри страны",
        "350 free domestic minutes": "350 бесплатных минут внутри страны",
        "350 free domestic SMS": "350 бесплатных SMS внутри страны",
        "800 free domestic minutes": "800 бесплатных минут внутри страны",
        "800 free domestic SMS": "800 бесплатных SMS внутри страны",
        "1200 free domestic minutes": "1200 бесплатных минут внутри страны",
        "1200 free domestic SMS": "1200 бесплатных SMS внутри страны",
        "No Plan": "Без тарифа",
        "Speeds": "Скорости",
        "1 min - 0.20 AZN": "1 мин - 0.20 AZN",
        "1 MB - 0.20 AZN, 4G+": "1 MB - 0.20 AZN, 4G+",
        "4G+ - up to 150 mbit/s (until 06.2026)": "4G+ - \u0434\u043e 150 \u041c\u0431\u0438\u0442/\u0441 (\u0434\u043e 06.2026)",
        "4.5G - up to 250 mbit/s (until 01.2027)": "4.5G - \u0434\u043e 250 \u041c\u0431\u0438\u0442/\u0441 (\u0434\u043e 01.2027)",
        "4.5G+ - up to 350 mbit/s": "4.5G+ - \u0434\u043e 350 \u041c\u0431\u0438\u0442/\u0441",
        "5G - up to 850 mbit/s": "5G - \u0434\u043e 850 \u041c\u0431\u0438\u0442/\u0441",
        "5G+ Beta from 06.2026 (only Ahmadli)": "5G+ Beta \u0441 06.2026 (\u0442\u043e\u043b\u044c\u043a\u043e \u0410\u0445\u043c\u0435\u0434\u043b\u044b)",
        "5G+ - up to 1500 mbit/s": "5G+ - \u0434\u043e 1500 \u041c\u0431\u0438\u0442/\u0441",
        "1 AZN - 7 days of all-network access": "1 AZN - 7 дней доступа ко всей сети",
        "up to": "до",
        "fiber internet": "оптический интернет",
        "Optical Internet": "Оптический интернет",
        "High-speed home and business connectivity with modern Wi-Fi standards and optional static IP.": "Высокоскоростное подключение для дома и бизнеса с современными стандартами Wi-Fi и опциональным статическим IP.",
        "Minimum Wi-Fi 6": "Минимум Wi-Fi 6",
        "Minimum Wi-Fi 6E": "Минимум Wi-Fi 6E",
        "Minimum Wi-Fi 7": "Минимум Wi-Fi 7",
        "Static IP": "Статический IP",
        "Add a static IP to any optical internet package.": "Добавьте статический IP к любому пакету оптического интернета.",
        "global beta": "global beta",
        "Global Beta": "Global Beta",
        "Travel data packages for early users with flexible 7-day and 30-day options.": "Туристические интернет-пакеты для ранних пользователей с гибкими вариантами на 7 и 30 дней.",
        "$5 | 90 days": "$5 | 90 дней",
        "25% off all plans for first users": "Скидка 25% на все тарифы для первых пользователей"
    },
    az: {
        "Plans": "Tariflər",
        "Global": "Qlobal",
        "Activation": "Aktivasiya",
        "Favorite": "Sevilən",
        "Support": "Dəstək",
        "Login": "Giriş",
        "Home": "Ana səhifə",
        "Instant mobile access": "Ani mobil bağlantı",
        "Activate your digital SIM with a QR code and stay connected without plastic cards.": "Rəqəmsal SIM-i QR kodla aktiv edin və plastik kart olmadan bağlı qalın.",
        "Fast setup": "Sürətli quraşdırma",
        "Scan, install, go online": "Skan edin, quraşdırın, qoşulun",
        "A smooth activation flow built for modern phones and everyday travel.": "Müasir telefonlar və gündəlik səyahət üçün rahat aktivasiya.",
        "Coverage": "Əhatə",
        "Made for Azerbaijan": "Azərbaycan üçün yaradılıb",
        "Reliable digital connectivity across the city, regions, and the road between them.": "Şəhərdə, bölgələrdə və yollar boyunca etibarlı rəqəmsal bağlantı.",
        "digital SIM for Azerbaijan": "Azərbaycan üçün rəqəmsal SIM",
        "Mobile connection without a plastic SIM card": "Plastik SIM kart olmadan mobil bağlantı",
        "Choose a plan, get your QR code, and activate El-Sim on your phone in just a few minutes.": "Tarif seçin, QR kod alın və El-Sim-i telefonunuzda bir neçə dəqiqəyə aktiv edin.",
        "Choose a plan": "Tarif seçin",
        "How it works": "Necə işləyir",
        "activation": "aktivasiya",
        "Three simple steps": "Üç sadə addım",
        "Pick the data package you need and pay online with a convenient method.": "Lazım olan internet paketini seçin və rahat üsulla onlayn ödəyin.",
        "Get your QR code": "QR kodunuzu alın",
        "Your activation code arrives by email right after purchase.": "Aktivasiya kodunuz alışdan dərhal sonra emailə gəlir.",
        "Connect": "Qoşulun",
        "Scan the QR code in your phone settings and stay online.": "Telefon ayarlarında QR kodu skan edin və onlayn qalın.",
        "plans": "tariflər",
        "Simple packages": "Sadə paketlər",
        "5 GB of mobile data": "5 GB mobil internet",
        "Choose": "Seç",
        "10 GB, calls and SMS": "10 GB, zənglər və SMS",
        "Unlimited mobile data": "Limitsiz mobil internet",
        "wi-fi fallback": "ehtiyat wi-fi",
        "Stay online between networks": "Şəbəkələr arasında onlayn qalın",
        "Use El-Sim support to find nearby Wi-Fi and finish activation when mobile coverage is weak.": "Mobil əhatə zəif olanda yaxın Wi-Fi tapmaq və aktivasiya üçün El-Sim dəstəyindən istifadə edin.",
        "Get help": "Yardım al",
        "support": "dəstək",
        "We can help you connect": "Qoşulmağa kömək edirik",
        "Message us if you need device checks, QR code help, or plan recommendations.": "Cihaz yoxlanışı, QR kod köməyi və ya tarif tövsiyəsi lazımdırsa bizə yazın.",
        "Contact us": "Əlaqə saxla",
        "Baku, Azerbaijan": "Bakı, Azərbaycan",
        "secure access": "təhlükəsiz giriş",
        "Manage your El-Sim profile": "El-Sim profilinizi idarə edin",
        "Sign in with your FIN and password, or create a new profile before choosing and activating your digital SIM.": "FIN və şifrə ilə daxil olun və ya rəqəmsal SIM seçməzdən əvvəl yeni profil yaradın.",
        "Fast activation": "Sürətli aktivasiya",
        "Plan history": "Tarif tarixçəsi",
        "Support access": "Dəstəyə giriş",
        "Register": "Qeydiyyat",
        "Name": "Ad",
        "Password": "Şifrə",
        "Exactly 7 English letters or digits.": "Dəqiq 7 ingilis hərfi və ya rəqəm.",
        "Minimum 8 characters.": "Minimum 8 simvol.",
        "Create account": "Hesab yarat",
        "membership": "üzvlük",
        "Unlock monthly El-Sim benefits, bonus data opportunities, partner rewards, and stronger network access.": "Aylıq El-Sim üstünlükləri, bonus internet imkanları, partnyor mükafatları və güclü şəbəkə əldə edin.",
        "4.00 AZN | 30 days": "4.00 AZN | 30 gün",
        "7.00 AZN | 30 days": "7.00 AZN | 30 gün",
        "11.00 AZN | 30 days": "11.00 AZN | 30 gün",
        "Up to 50 free calls": "50-dək pulsuz zənglər",
        "Bonuses from our partners": "Partnyorlarımızdan bonuslar",
        "Opportunity to earn up to 5GB": "5GB-dək qazanmaq imkanı",
        "Connection to next-generation 5G internet": "5G nəsil internetə qoşulma",
        "Up to 50% ad blocking": "50%-dək reklamların bloklanması",
        "5G+ internet connection": "5G+ internetə qoşulma",
        "Up to 100 free calls": "100-dək pulsuz zənglər",
        "Opportunity to earn up to 10GB": "10GB-dək qazanmaq imkanı",
        "Higher bonuses from partners": "Partnyorlardan yüksək bonuslar",
        "Up to 70% ad blocking": "70%-dək reklamların bloklanması",
        "VIP customer service": "VIP müştəri xidməti",
        "Up to 150 free calls": "150-dək pulsuz zənglər",
        "Opportunity to earn up to 15GB": "15GB-dək qazanmaq imkanı",
        "Ultra bonuses from partners": "Partnyorlardan ultra bonuslar",
        "Ability to change the app icon": "App icon-u dəyişdirmək imkanı",
        "Up to 90% ad blocking": "90%-dək reklamların bloklanması",
        "Network access anywhere in the Republic of Azerbaijan": "AR istənilən ərazisində şəbəkə imkanı",
        "tariffs": "tariflər",
        "Choose a monthly package with local minutes, local SMS, and mobile internet for everyday connection.": "Gündəlik bağlantı üçün ölkədaxili dəqiqə, SMS və mobil internetli aylıq paket seçin.",
        "eSIM price": "eSIM qiyməti",
        "50% off all plans for the first month": "İlk ay bütün tariflərə 50% endirim",
        "30 days": "30 gün",
        "7 days": "7 gün",
        "30 free domestic minutes": "30 dəq ölkədaxili pulsuz",
        "30 free domestic SMS": "30 SMS ölkədaxili pulsuz",
        "50 free domestic minutes": "50 dəq ölkədaxili pulsuz",
        "50 free domestic SMS": "50 SMS ölkədaxili pulsuz",
        "150 free domestic minutes": "150 dəq ölkədaxili pulsuz",
        "150 free domestic SMS": "150 SMS ölkədaxili pulsuz",
        "350 free domestic minutes": "350 dəq ölkədaxili pulsuz",
        "350 free domestic SMS": "350 SMS ölkədaxili pulsuz",
        "800 free domestic minutes": "800 dəq ölkədaxili pulsuz",
        "800 free domestic SMS": "800 SMS ölkədaxili pulsuz",
        "1200 free domestic minutes": "1200 dəq ölkədaxili pulsuz",
        "1200 free domestic SMS": "1200 SMS ölkədaxili pulsuz",
        "No Plan": "Tarifsiz",
        "Speeds": "Sürətlər",
        "1 min - 0.20 AZN": "1 dəq - 0.20 AZN",
        "1 MB - 0.20 AZN, 4G+": "1 MB - 0.20 AZN, 4G+",
        "4G+ - up to 150 mbit/s (until 06.2026)": "4G+ - 150 Mbit/s-dək (06.2026-ə qədər)",
        "4.5G - up to 250 mbit/s (until 01.2027)": "4.5G - 250 Mbit/s-dək (01.2027-ə qədər)",
        "4.5G+ - up to 350 mbit/s": "4.5G+ - 350 Mbit/s-dək",
        "5G - up to 850 mbit/s": "5G - 850 Mbit/s-dək",
        "5G+ Beta from 06.2026 (only Ahmadli)": "5G+ Beta 06.2026-dan (yalnız Əhmədlidə)",
        "5G+ - up to 1500 mbit/s": "5G+ - 1500 Mbit/s-dək",
        "1 AZN - 7 days of all-network access": "1 AZN - 7 gün hərtərəfli şəbəkə",
        "fiber internet": "optik internet",
        "Optical Internet": "Optik internet",
        "High-speed home and business connectivity with modern Wi-Fi standards and optional static IP.": "Müasir Wi-Fi standartları və istəyə bağlı statik IP ilə ev və biznes üçün yüksək sürətli bağlantı.",
        "Minimum Wi-Fi 6": "Min. Wi-Fi 6",
        "Minimum Wi-Fi 6E": "Min. Wi-Fi 6E",
        "Minimum Wi-Fi 7": "Min. Wi-Fi 7",
        "Static IP": "Statik IP",
        "Add a static IP to any optical internet package.": "İstənilən optik internet paketinə statik IP əlavə edin.",
        "global beta": "global beta",
        "Travel data packages for early users with flexible 7-day and 30-day options.": "İlk istifadəçilər üçün 7 və 30 günlük çevik internet paketləri.",
        "$5 | 90 days": "$5 | 90 gün",
        "25% off all plans for first users": "İlk istifadəçilər üçün bütün tariflərdə 25% endirim"
    }
};

Object.assign(translations.ru, {
    "user profile": "\u043f\u0440\u043e\u0444\u0438\u043b\u044c",
    "Profile image": "\u0418\u0437\u043e\u0431\u0440\u0430\u0436\u0435\u043d\u0438\u0435 \u043f\u0440\u043e\u0444\u0438\u043b\u044f",
    "Drop image here": "\u041f\u0435\u0440\u0435\u0442\u0430\u0449\u0438\u0442\u0435 \u0438\u0437\u043e\u0431\u0440\u0430\u0436\u0435\u043d\u0438\u0435 \u0441\u044e\u0434\u0430",
    "or choose GIF, PNG, JPG": "\u0438\u043b\u0438 \u0432\u044b\u0431\u0435\u0440\u0438\u0442\u0435 GIF, PNG, JPG",
    "or choose GIF, PNG, JPG up to 2 MB": "\u0438\u043b\u0438 \u0432\u044b\u0431\u0435\u0440\u0438\u0442\u0435 GIF, PNG, JPG \u0434\u043e 2 \u041c\u0411",
    "File is larger than 2 MB": "\u0424\u0430\u0439\u043b \u0431\u043e\u043b\u044c\u0448\u0435 2 \u041c\u0411",
    "Profile image must be 2 MB or less.": "\u0418\u0437\u043e\u0431\u0440\u0430\u0436\u0435\u043d\u0438\u0435 \u043f\u0440\u043e\u0444\u0438\u043b\u044f \u0434\u043e\u043b\u0436\u043d\u043e \u0431\u044b\u0442\u044c \u043d\u0435 \u0431\u043e\u043b\u044c\u0448\u0435 2 \u041c\u0411.",
    "Hover to preview": "\u041d\u0430\u0432\u0435\u0434\u0438\u0442\u0435 \u0434\u043b\u044f \u043f\u0440\u0435\u0434\u043f\u0440\u043e\u0441\u043c\u043e\u0442\u0440\u0430",
    "Clear field": "\u041e\u0447\u0438\u0441\u0442\u0438\u0442\u044c \u043f\u043e\u043b\u0435",
    "Upload image": "\u0417\u0430\u0433\u0440\u0443\u0437\u0438\u0442\u044c",
    "Delete image": "\u0423\u0434\u0430\u043b\u0438\u0442\u044c \u0438\u0437\u043e\u0431\u0440\u0430\u0436\u0435\u043d\u0438\u0435",
    "Delete profile image?": "\u0423\u0434\u0430\u043b\u0438\u0442\u044c \u0438\u0437\u043e\u0431\u0440\u0430\u0436\u0435\u043d\u0438\u0435 \u043f\u0440\u043e\u0444\u0438\u043b\u044f?",
    "This will remove your current profile image.": "\u0422\u0435\u043a\u0443\u0449\u0435\u0435 \u0438\u0437\u043e\u0431\u0440\u0430\u0436\u0435\u043d\u0438\u0435 \u043f\u0440\u043e\u0444\u0438\u043b\u044f \u0431\u0443\u0434\u0435\u0442 \u0443\u0434\u0430\u043b\u0435\u043d\u043e.",
    "Cancel": "\u041e\u0442\u043c\u0435\u043d\u0430",
    "Email": "Email",
    "Enable 2FA by email": "\u0412\u043a\u043b\u044e\u0447\u0438\u0442\u044c 2FA \u043f\u043e email",
    "Email notifications": "\u0423\u0432\u0435\u0434\u043e\u043c\u043b\u0435\u043d\u0438\u044f \u043d\u0430 email",
    "Save profile": "\u0421\u043e\u0445\u0440\u0430\u043d\u0438\u0442\u044c \u043f\u0440\u043e\u0444\u0438\u043b\u044c",
    "Created": "\u0421\u043e\u0437\u0434\u0430\u043d",
    "Logout": "\u0412\u044b\u0439\u0442\u0438",
    "Two-factor verification": "\u0414\u0432\u0443\u0445\u0444\u0430\u043a\u0442\u043e\u0440\u043d\u0430\u044f \u043f\u0440\u043e\u0432\u0435\u0440\u043a\u0430",
    "email verification": "\u043f\u0440\u043e\u0432\u0435\u0440\u043a\u0430 email",
    "Enter your": "\u0412\u0432\u0435\u0434\u0438\u0442\u0435",
    "7-digit code": "7-\u0437\u043d\u0430\u0447\u043d\u044b\u0439 \u043a\u043e\u0434",
    "Enter your 7-digit code": "\u0412\u0432\u0435\u0434\u0438\u0442\u0435 7-\u0437\u043d\u0430\u0447\u043d\u044b\u0439 \u043a\u043e\u0434",
    "The code is valid for 15 minutes. Used and expired codes are removed automatically.": "\u041a\u043e\u0434 \u0434\u0435\u0439\u0441\u0442\u0432\u0443\u0435\u0442 15 \u043c\u0438\u043d\u0443\u0442. \u0418\u0441\u043f\u043e\u043b\u044c\u0437\u043e\u0432\u0430\u043d\u043d\u044b\u0435 \u0438 \u0438\u0441\u0442\u0435\u043a\u0448\u0438\u0435 \u043a\u043e\u0434\u044b \u0443\u0434\u0430\u043b\u044f\u044e\u0442\u0441\u044f \u0430\u0432\u0442\u043e\u043c\u0430\u0442\u0438\u0447\u0435\u0441\u043a\u0438.",
    "Code": "\u041a\u043e\u0434",
    "Enter exactly 7 digits from the email.": "\u0412\u0432\u0435\u0434\u0438\u0442\u0435 \u0440\u043e\u0432\u043d\u043e 7 \u0446\u0438\u0444\u0440 \u0438\u0437 email.",
    "Verify": "\u041f\u043e\u0434\u0442\u0432\u0435\u0440\u0434\u0438\u0442\u044c"
});

Object.assign(translations.az, {
    "user profile": "istifad\u0259\u00e7i profili",
    "Profile image": "Profil \u015f\u0259kli",
    "Drop image here": "\u015e\u0259kli bura at\u0131n",
    "or choose GIF, PNG, JPG": "v\u0259 ya GIF, PNG, JPG se\u00e7in",
    "or choose GIF, PNG, JPG up to 2 MB": "v\u0259 ya 2 MB-d\u0259k GIF, PNG, JPG se\u00e7in",
    "File is larger than 2 MB": "Fayl 2 MB-dan b\u00f6y\u00fckd\u00fcr",
    "Profile image must be 2 MB or less.": "Profil \u015f\u0259kli 2 MB v\u0259 ya daha az olmal\u0131d\u0131r.",
    "Hover to preview": "\u00d6n bax\u0131\u015f \u00fc\u00e7\u00fcn \u00fcst\u00fcn\u0259 g\u0259lin",
    "Clear field": "Sah\u0259ni t\u0259mizl\u0259",
    "Upload image": "\u015e\u0259kli y\u00fckl\u0259",
    "Delete image": "\u015e\u0259kli sil",
    "Delete profile image?": "Profil \u015f\u0259klini silm\u0259k ist\u0259yirsiniz?",
    "This will remove your current profile image.": "Cari profil \u015f\u0259kliniz silin\u0259c\u0259k.",
    "Cancel": "L\u0259\u011fv et",
    "Email": "Email",
    "Enable 2FA by email": "Email il\u0259 2FA aktiv et",
    "Email notifications": "Email bildiri\u015fl\u0259ri",
    "Save profile": "Profili saxla",
    "Created": "Yarad\u0131l\u0131b",
    "Logout": "\u00c7\u0131x\u0131\u015f",
    "Two-factor verification": "\u0130ki faktorlu t\u0259sdiq",
    "email verification": "email t\u0259sdiqi",
    "Enter your": "Daxil edin",
    "7-digit code": "7 r\u0259q\u0259mli kodu",
    "Enter your 7-digit code": "7 r\u0259q\u0259mli kodu daxil edin",
    "The code is valid for 15 minutes. Used and expired codes are removed automatically.": "Kod 15 d\u0259qiq\u0259 etibarl\u0131d\u0131r. \u0130stifad\u0259 edilmi\u015f v\u0259 vaxt\u0131 bitmi\u015f kodlar avtomatik silinir.",
    "Code": "Kod",
    "Enter exactly 7 digits from the email.": "Emaild\u0259ki d\u0259qiq 7 r\u0259q\u0259mi daxil edin.",
    "Verify": "T\u0259sdiql\u0259"
});

const originalText = new WeakMap();

const getTranslation = (text, language) => {
    if (language === "en") {
        return text;
    }

    const dictionary = translations[language] || {};
    let translated = dictionary[text];

    if (!translated) {
        translated = text
            .replace(/\| 30 days/g, `| ${dictionary["30 days"] || "30 days"}`)
            .replace(/\| 7 days/g, `| ${dictionary["7 days"] || "7 days"}`)
            .replace(/up to/g, dictionary["up to"] || "up to");
    }

    return translated;
};

const applyLanguage = (language) => {
    const currentLanguage = translations[language] || language === "en" ? language : "en";
    const walker = document.createTreeWalker(document.body, NodeFilter.SHOW_TEXT);

    while (walker.nextNode()) {
        const node = walker.currentNode;
        const parent = node.parentElement;

        if (!parent || ["SCRIPT", "STYLE", "SVG"].includes(parent.tagName)) {
            continue;
        }

        const text = node.textContent.trim();

        if (!text) {
            continue;
        }

        if (!originalText.has(node)) {
            originalText.set(node, text);
        }

        const original = originalText.get(node);
        const translated = getTranslation(original, currentLanguage);
        node.textContent = node.textContent.replace(text, translated);
    }

    document.documentElement.lang = currentLanguage;
    document.querySelectorAll("[data-lang-button]").forEach((button) => {
        button.classList.toggle("is-active", button.dataset.langButton === currentLanguage);
    });
};

const savedLanguage = localStorage.getItem(languageStorageKey) || "en";

document.querySelectorAll("[data-lang-button]").forEach((button) => {
    button.addEventListener("click", () => {
        localStorage.setItem(languageStorageKey, button.dataset.langButton);
        applyLanguage(button.dataset.langButton);
    });
});

applyLanguage(savedLanguage);

const profileImageConfirmModal = document.querySelector("[data-profile-image-confirm]");
let pendingProfileImageDeleteForm = null;

profileImageConfirmModal?.querySelector("[data-profile-confirm-cancel]")?.addEventListener("click", () => {
    pendingProfileImageDeleteForm = null;
    profileImageConfirmModal.close();
});

profileImageConfirmModal?.querySelector("[data-profile-confirm-delete]")?.addEventListener("click", () => {
    const form = pendingProfileImageDeleteForm;
    profileImageConfirmModal.close();
    form?.requestSubmit();
});

document.querySelectorAll("[data-confirm-delete-image]").forEach((form) => {
    form.addEventListener("submit", (event) => {
        if (pendingProfileImageDeleteForm === form) {
            pendingProfileImageDeleteForm = null;
            return;
        }

        event.preventDefault();

        if (profileImageConfirmModal?.showModal) {
            pendingProfileImageDeleteForm = form;
            profileImageConfirmModal.showModal();
            return;
        }

        window.showConfirmModal?.({
            message: getTranslation("Delete profile image?", localStorage.getItem(languageStorageKey) || "en"),
            button: getTranslation("Delete image", localStorage.getItem(languageStorageKey) || "en")
        }).then((confirmed) => {
            if (confirmed) {
                pendingProfileImageDeleteForm = form;
                form.requestSubmit();
            }
        });
    });
});
