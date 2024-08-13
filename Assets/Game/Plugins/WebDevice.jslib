// WebGL: ブラウザースクリプトとの対話
// https://docs.unity.cn/ja/current/Manual/webgl-interactingwithbrowserscripting.html

mergeInto(LibraryManager.library, {

  // モバイルの種類の取得 (0: モバイル以外, 1: Android, 2: iOS)
  // (参考: https://qiita.com/kazuki_kuriyama/items/86bfb7db1b30ddfecdb4)
  getMobileType: function () {
    var userAgent = window.navigator.userAgent.toLowerCase();

    var isAndroid = userAgent.search(/android/i) !== -1;
    if (isAndroid) return 1;
    
    var isIos = userAgent.search(/iphone|ipad|ipod/i) !== -1 || (userAgent.indexOf("macintosh") !== -1 && "ontouchend" in document);
    if (isIos) return 2;

    return 0;
  },
  
});
