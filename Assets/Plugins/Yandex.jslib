mergeInto(LibraryManager.library, {

  // --- Инициализация (Game Ready) ---
  GameReady: function () {
    if (typeof ysdk !== 'undefined' && ysdk !== null) {
        ysdk.features.LoadingAPI.ready();
        console.log('Yandex SDK: Game Ready Reported');
    } else {
        console.error('Yandex SDK not initialized yet');
    }
  },

  // --- Реклама за вознаграждение (Reward) ---
  ShowYandexRewardAd: function () {
    if (typeof ysdk !== 'undefined' && ysdk !== null) {
      ysdk.adv.showRewardedVideo({
        callbacks: {
          onOpen: function () {
            console.log('Reward Ad Open');
            // Сообщаем Unity, что реклама открылась (пауза)
            if (window.myGameInstance) window.myGameInstance.SendMessage('YandexManager', 'OnAdOpen');
          },
          onRewarded: function () {
            console.log('Reward Granted');
            // Сообщаем Unity, что награду можно выдавать
            if (window.myGameInstance) window.myGameInstance.SendMessage('YandexManager', 'OnRewardedAdReward');
          },
          onClose: function () {
            console.log('Reward Ad Closed');
            // Сообщаем Unity, что реклама закрылась (снять паузу)
            if (window.myGameInstance) window.myGameInstance.SendMessage('YandexManager', 'OnAdClose');
          },
          onError: function (e) {
            console.log('Error while open video ad:', e);
            // Если ошибка, тоже снимаем паузу, чтобы игра не зависла
            if (window.myGameInstance) window.myGameInstance.SendMessage('YandexManager', 'OnAdClose');
          }
        }
      });
    }
  },

  // --- Межстраничная реклама (Interstitial) ---
  ShowYandexInterstitialAd: function () {
    if (typeof ysdk !== 'undefined' && ysdk !== null) {
      ysdk.adv.showFullscreenAdv({
        callbacks: {
          onOpen: function () {
            console.log('Interstitial Ad Open');
            if (window.myGameInstance) window.myGameInstance.SendMessage('YandexManager', 'OnAdOpen');
          },
          onClose: function (wasShown) {
            console.log('Interstitial Ad Closed');
            if (window.myGameInstance) window.myGameInstance.SendMessage('YandexManager', 'OnAdClose');
          },
          onError: function (error) {
            console.log('Error while open fullscreen ad:', error);
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
        player.setData(myObj).then(() => {
            console.log('Data saved to Yandex');
        });
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
        // Если игрока нет (не авторизован), пробуем инициализировать
        if (typeof ysdk !== 'undefined' && ysdk !== null) {
            ysdk.getPlayer().then(_player => {
                player = _player;
                player.getData().then((data) => {
                    var jsonString = JSON.stringify(data);
                    if (window.myGameInstance) 
                        window.myGameInstance.SendMessage('YandexManager', 'OnLoadDataReceived', jsonString);
                });
            }).catch(err => {
                console.log('Player initialization error or guest mode');
                // Отправляем пустой json чтобы игра продолжилась
                if (window.myGameInstance) 
                    window.myGameInstance.SendMessage('YandexManager', 'OnLoadDataReceived', "{}");
            });
        }
    }
  }
});