mergeInto(LibraryManager.library, {

  // --- Инициализация (Game Ready) ---
  GameReady: function () {
    if (typeof ysdk !== 'undefined' && ysdk !== null) {
        ysdk.features.LoadingAPI.ready();
        console.log('Yandex SDK: Game Ready Reported');
    }
  },

  // --- Проверка, загрузился ли SDK ---
  CheckYandexSDKReady: function () {
    if (typeof ysdk !== 'undefined' && ysdk !== null) {
        return true;
    }
    return false;
  },

  // --- Автоопределение языка (Пункт 2.14) ---
  GetLang: function () {
    if (typeof ysdk !== 'undefined' && ysdk !== null) {
      var lang = ysdk.environment.i18n.lang;
      var bufferSize = lengthBytesUTF8(lang) + 1;
      var buffer = _malloc(bufferSize);
      stringToUTF8(lang, buffer, bufferSize);
      return buffer;
    }
    return null;
  },

  // --- Реклама за вознаграждение ---
  ShowYandexRewardAd: function () {
    if (typeof ysdk !== 'undefined' && ysdk !== null) {
      ysdk.adv.showRewardedVideo({
        callbacks: {
          onOpen: function () {
            if (window.myGameInstance) window.myGameInstance.SendMessage('YandexManager', 'OnAdOpen');
          },
          onRewarded: function () {
            if (window.myGameInstance) window.myGameInstance.SendMessage('YandexManager', 'OnRewardedAdReward');
          },
          onClose: function () {
            if (window.myGameInstance) window.myGameInstance.SendMessage('YandexManager', 'OnAdClose');
          },
          onError: function (e) {
            if (window.myGameInstance) window.myGameInstance.SendMessage('YandexManager', 'OnAdClose');
          }
        }
      });
    }
  },

  // --- Межстраничная реклама ---
  ShowYandexInterstitialAd: function () {
    if (typeof ysdk !== 'undefined' && ysdk !== null) {
      ysdk.adv.showFullscreenAdv({
        callbacks: {
          onOpen: function () {
            if (window.myGameInstance) window.myGameInstance.SendMessage('YandexManager', 'OnAdOpen');
          },
          onClose: function (wasShown) {
            if (window.myGameInstance) window.myGameInstance.SendMessage('YandexManager', 'OnAdClose');
          },
          onError: function (error) {
            if (window.myGameInstance) window.myGameInstance.SendMessage('YandexManager', 'OnAdClose');
          }
        }
      });
    }
  },

  // --- Сохранение данных (Cloud Save) ---
  SaveToYandex: function (data) {
    var dataString = UTF8ToString(data);
    var myObj = JSON.parse(dataString);
    
    if (typeof player !== 'undefined' && player !== null) {
        // Мы используем flush: true для надежности сохранения
        player.setData(myObj, {flush: true}).then(() => {
            console.log('Data saved to Yandex');
        }).catch(err => {
            console.error('Save error:', err);
        });
    } else {
        console.warn('Player not initialized for saving');
    }
  },

  // --- Загрузка данных ---
  LoadFromYandex: function () {
    if (typeof player !== 'undefined' && player !== null) {
        player.getData().then((data) => {
            var jsonString = JSON.stringify(data);
            if (window.myGameInstance) 
                window.myGameInstance.SendMessage('YandexManager', 'OnLoadDataReceived', jsonString);
        });
    } else {
        if (typeof ysdk !== 'undefined' && ysdk !== null) {
            ysdk.getPlayer({scopes: false}).then(_player => {
                player = _player;
                player.getData().then((data) => {
                    var jsonString = JSON.stringify(data);
                    if (window.myGameInstance) 
                        window.myGameInstance.SendMessage('YandexManager', 'OnLoadDataReceived', jsonString);
                });
            }).catch(err => {
                console.log('Guest mode or player error');
                if (window.myGameInstance) 
                    window.myGameInstance.SendMessage('YandexManager', 'OnLoadDataReceived', "{}");
            });
        }
    }
  }
});