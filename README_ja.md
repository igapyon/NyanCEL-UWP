# NyanCEL-UWP

NyanCEL-UWP は Excel ブック (.xlsx) のシート内容を SQL で検索できるようにするサーバーツールです。

- NyanCEL-UWP は UWP と C# で実装されています。
- Excel ブック (.xlsx) を与えることにより、各シートをテーブルに見立てて SQL で検索できるようにします。
- REST インタフェースを経由して SQL を実行して結果を取得します。
- MIT ライセンスのもとで公開されています。
- NyanCEL-UWP は NyanCEL プロジェクトの一部です。

## NyanQL プロジェクトとの関連性

- NyanCEL は、にゃんくるプロジェクトのフレンドプロジェクトです。
- にゃんくるプロジェクトに感銘を受けて開発された独自プロジェクトです。
- NyanCEL は、にゃんくるプロジェクトを尊敬しており、また尊重しています。
- UWP + C#で実装されており、Microsoft Store で配布する計画です。

## 動作の流れ

1. Excelブック（.xlsx）ファイルを与えます
   - シート名をテーブル名として使用
   - 1行目のセル値を項目名として使用
   - 2行目のセル書式から項目データ型を導出
2. 指定した Excel ブックの内容を SQLite データベースに読み込みます
   - 2行目からデータを読み込み
   - 読み込んだデータをメモリ上の SQLite にロード
3. RESTインターフェースからSELECT文を実行します。
  - 与えられた SELECT 文でデータベースを検索
  - SQLite の SQL 文法が利用可能
  - GETおよびPOSTメソッドが利用可能です。(現状はGETのみ対応)
  - (デフォルトの) ポート番号は28096です。
4. SELECT文の検索結果を返却します。
  - 行データを繰り返して検索結果を返却
  - 返却結果として、json, xml, xlsx 形式サポート
  - デフォルトで json データとして検索結果を返却
  - fmt=xml パラメータ追加でデータ返却形式を XML に変更
  - fmt=xlsx パラメータ追加でデータ返却形式を xlsx に変更
  - fmt=json&target=data.1 パラメータ指定で返却データを絞って指定
  - fmt=json&jsonpath= で検索結果に対して jsonpath を適用
  - fmt=xml&xpath= で検索結果に対して xpath を適用

UWPの機能として、デスクトップとリムーバブルディスクへのアクセスのみが設定されています。

## 内部的に利用している OSS

NyanCEL-UWP は以下の OSS を内部的に利用しています。各 OSS の提供者に感謝します。

- ClosedXML
  - MIT
  - 0.102.2
- EmbedIO
  - MIT
  - 3.5.2
- Microsoft.Data.Sqlite
  - MIT
  - 8.0.6
- Microsoft.UI.Xaml
  - 2.8.6
- Newtonsoft.Json
  - MIT
  - 13.0.3
- Serilog
  - MIT
  - 4.0.0
- Serilog.Sinks.File
  - MIT
  - 5.0.0
- Igapyon.NyanCEL
  - MIT
  - 0.5.0

## 動作ログのパス

次のフォルダ階層に、NyanCELの動作ログが格納されます。

```sh
USERROOTPATH\AppData\Local\Packages\NyanCEL-XXXXXXXXXXXXX\LocalState
```

- 動作に関するログファイルと、実行したSQLのログファイルが作成されます。

## 動作確認に便利なSQL

```sh
http://IPADDRESS:28096/api?sql=SELECT%20*%20FROM%20sqlite_master
```

# 制限

- オンメモリRDBMSで動作するため、大量のデータを与えた場合に動作しない可能性があります。
- .xlsx ファイルにのみ対応します。
- ループバックによるHTTP接続ができません。別のマシンからアクセスしてください。
- アプリは基本的にはフォアグラウンドで動作することを想定します。
- Excelのシート名やタイトル行の列名にダブルクオートを含めることはできません。

# Install

- NyanCEL_1.0.1.0_x86_x64_arm_arm64.cer を準備
- certlm.msc を使用して「信頼されたルート証明機関 > 証明書」を開く
- 右クリック > すべてのタスク > インポート。証明書（NyanCEL_1.0.1.0_x86_x64_arm_arm64.cer）をインストール
- NyanCEL_1.0.1.0_x86_x64_arm_arm64.msixbundle をダブルクリックしてインストール


# TODO

- (ASAP) BASIC認証のユーザー＋パスワードを起動時に指定できるようにする
- (ASAP) ユーザ指定の証明書を利用して https で動作させる機能。
- 動作境界系
  - エラー対応。Excel ブック読み込み失敗対応など（Excel 以外のデータを入力した場合）。ドキュメント・リムーバブル以外からのファイル読み込みエラー確認。
- 見栄え
  - アプリのストア用の画像ファイルを妥当なものに更新する
  - README.md をちゃんとする
- テスト
  - テストケース作る
  - 正式なコードサイン鍵の入手
  - KIOSKモードで動作するか確認
  - ストアにベータ版を掲載してみる
  - 同名の列が存在する xlsx ファイルの投入
  - NyanRowId を含んだ xlsx ファイルの投入
  - １行目がnullな xlsx ファイルの投入
- 将来バージョンの機能
  - NyanQL の設定ファイルとの一定の互換性確保
  - DML を投入可能なAPI (/dml) を追加
  - オンメモリ上のデータをエクスポートする API (/exp) を追加
  - ポート番号変更機能
  - URIスキーム起動: 引数で Excel ブックやポート番号など指定可能に
  - SQL logの出力 ON/OFF 機能
  - CSV対応
