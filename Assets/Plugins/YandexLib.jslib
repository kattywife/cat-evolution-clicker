mergeInto(LibraryManager.library, {

  // --- 1. ÇÀÃÐÓÇÊÀ ÈÃÐÛ ---
  GameReady: function () {
    try {
        if (typeof ysdk !== 'undefined') {
            ysdk.features.LoadingAPI.ready();
            console.log('Game Ready sent');
        }
    } catch (e) { console.error(e); }
  },

  // --- 2. ÑÎÕÐÀÍÅÍÈß (Cloud Saves) ---
  // Ñîõðàíèòü äàííûå (ñòðîêà JSON)
  SaveToYandex: function (dateString) {
    try {
        if (typeof ysdk !== 'undefined') {
            var dataString = UTF8ToString(dateString);
            var myObj = JSON.parse(dataString);
            ysdk.getPlayer().then(player => {
                player.setData(myObj).then(() => {
                    console.log('Data saved');
                });
            });
        }
    } catch (e) { console.error(e); }
  },

  // Çàãðóçèòü äàííûå
  LoadFromYandex: function () {
    try {
        if (typeof ysdk !== 'undefined') {
            ysdk.getPlayer().then(player => {
                player.getData().then(data => {
                    var jsonString = JSON.stringify(data);
                    // Îòïðàâëÿåì äàííûå îáðàòíî â Unity
                    myGameInstance.SendMessage('YandexManager', 'OnLoadDataReceived', jsonString);
                });
            });
        } else {
            // Åñëè SDK íåò, îòïðàâëÿåì ïóñòîòó, ÷òîáû Unity çíàëà, ÷òî îòâåòà íå áóäåò
             myGameInstance.SendMessage('YandexManager', 'OnLoadDataReceived', "");
        }
    } catch (e) { 
        console.error(e);
        myGameInstance.SendMessage('YandexManager', 'OnLoadDataReceived', "");
    }
  },

  // --- 3. ÐÅÊËÀÌÀ ÇÀ ÂÎÇÍÀÃÐÀÆÄÅÍÈÅ (Rewarded) ---
  ShowYandexRewardAd: function () {
    try {
        if (typeof ysdk !== 'undefined') {
            ysdk.adv.showRewardedVideo({
                callbacks: {
                    onOpen: () => { myGameInstance.SendMessage('YandexManager', 'OnAdOpen'); },
                    onRewarded: () => { myGameInstance.SendMessage('YandexManager', 'OnRewardedAdReward'); },
                    onClose: () => { myGameInstance.SendMessage('YandexManager', 'OnAdClose'); },
                    onError: (e) => { myGameInstance.SendMessage('YandexManager', 'OnAdClose'); }
                }
            });
        }
    } catch (e) { console.error(e); myGameInstance.SendMessage('YandexManager', 'OnAdClose'); }
  },

  // --- 4. ÌÅÆÑÒÐÀÍÈ×ÍÀß ÐÅÊËÀÌÀ (Interstitial) ---
  ShowYandexInterstitialAd: function () {
    try {
        if (typeof ysdk !== 'undefined') {
            ysdk.adv.showFullscreenAdv({
                callbacks: {
                    onOpen: () => { myGameInstance.SendMessage('YandexManager', 'OnAdOpen'); },
                    onClose: () => { myGameInstance.SendMessage('YandexManager', 'OnAdClose'); },
                    onError: (e) => { myGameInstance.SendMessage('YandexManager', 'OnAdClose'); }
                }
            });
        }
    } catch (e) { console.error(e); }
  },

});