# 360VideoToPictures
This application cuts out images in four directions from a 360-degree video.

360度動画から4方向の画像を切り出すアプリケーションです。

主にフォトグラメトリやNeRFへの活用を想定しています。

特徴は以下の通りです。
- 切り出す際の水平視野角を設定可能
- 切り出すフレームレートを設定可能
注意事項は以下の通りです
- 対応している動画は現状mp4のみ
- 最大解像度は5760×2880

# 使い方
1. 画像を保存する場所を選ぶ
2. 対象とする動画を選ぶ
3. カメラの水平視野角を指定する
4. 切り出すフレームレートを指定する
5. Startボタンを押す
![image](https://user-images.githubusercontent.com/52432227/225943348-062dffca-6830-4834-9007-a8069f4fd3ac.png)

# Tips
- NeRF等で活用する場合は、水平視野角は95度以上が良さそうです。
- 切り出すフレームレートは動画長によって調整し、適切な枚数になるようにご注意ください。

関連事項を[こちらの記事](https://qiita.com/Kazu_Sack/items/d3e725f60bd1fc360f4e)で書いています。実装内容は記事からアップデートしています。

# Lisence
This project is licensed under the MIT License, see the LICENSE.txt file for details.

This project includes the following open source projects:

[Unity Standalone File Browser](https://github.com/gkngkc/UnityStandaloneFileBrowser): Copyright (c) 2017 Gökhan Gökçe
