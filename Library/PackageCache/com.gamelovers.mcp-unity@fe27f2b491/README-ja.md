# MCP Unity エディター (ゲームエンジン)

[![](https://badge.mcpx.dev?status=on 'MCP 有効')](https://modelcontextprotocol.io/introduction)
[![](https://img.shields.io/badge/Unity-000000?style=flat&logo=unity&logoColor=white 'Unity')](https://unity.com/releases/editor/archive)
[![](https://img.shields.io/badge/Node.js-339933?style=flat&logo=nodedotjs&logoColor=white 'Node.js')](https://nodejs.org/en/download/)

[![smithery badge](https://smithery.ai/badge/@CoderGamester/mcp-unity)](https://smithery.ai/server/@CoderGamester/mcp-unity)
[![](https://img.shields.io/github/stars/CoderGamester/mcp-unity 'Stars')](https://github.com/CoderGamester/mcp-unity/stargazers)
[![](https://img.shields.io/github/last-commit/CoderGamester/mcp-unity 'Last Commit')](https://github.com/CoderGamester/mcp-unity/commits/main)
[![](https://img.shields.io/badge/License-MIT-red.svg 'MIT License')](https://opensource.org/licenses/MIT)

| [English](README.md) | [🇨🇳简体中文](README_zh-CN.md) | [🇯🇵日本語](README-ja.md) |
|----------------------|---------------------------------|----------------------|

```                                                                        
                              ,/(/.   *(/,                                  
                          */(((((/.   *((((((*.                             
                     .*((((((((((/.   *((((((((((/.                         
                 ./((((((((((((((/    *((((((((((((((/,                     
             ,/(((((((((((((/*.           */(((((((((((((/*.                
            ,%%#((/((((((*                    ,/(((((/(#&@@(                
            ,%%##%%##((((((/*.             ,/((((/(#&@@@@@@(                
            ,%%######%%##((/(((/*.    .*/(((//(%@@@@@@@@@@@(                
            ,%%####%#(%%#%%##((/((((((((//#&@@@@@@&@@@@@@@@(                
            ,%%####%(    /#%#%%%##(//(#@@@@@@@%,   #@@@@@@@(                
            ,%%####%(        *#%###%@@@@@@(        #@@@@@@@(                
            ,%%####%(           #%#%@@@@,          #@@@@@@@(                
            ,%%##%%%(           #%#%@@@@,          #@@@@@@@(                
            ,%%%#*              #%#%@@@@,             *%@@@(                
            .,      ,/##*.      #%#%@@@@,     ./&@#*      *`                
                ,/#%#####%%#/,  #%#%@@@@, ,/&@@@@@@@@@&\.                    
                 `*#########%%%%###%@@@@@@@@@@@@@@@@@@&*´                   
                    `*%%###########%@@@@@@@@@@@@@@&*´                        
                        `*%%%######%@@@@@@@@@@&*´                            
                            `*#%%##%@@@@@&*´                                 
                               `*%#%@&*´                                     
                                                        
     ███╗   ███╗ ██████╗██████╗         ██╗   ██╗███╗   ██╗██╗████████╗██╗   ██╗
     ████╗ ████║██╔════╝██╔══██╗        ██║   ██║████╗  ██║██║╚══██╔══╝╚██╗ ██╔╝
     ██╔████╔██║██║     ██████╔╝        ██║   ██║██╔██╗ ██║██║   ██║    ╚████╔╝ 
     ██║╚██╔╝██║██║     ██╔═══╝         ██║   ██║██║╚██╗██║██║   ██║     ╚██╔╝  
     ██║ ╚═╝ ██║╚██████╗██║             ╚██████╔╝██║ ╚████║██║   ██║      ██║   
     ╚═╝     ╚═╝ ╚═════╝╚═╝              ╚═════╝ ╚═╝  ╚═══╝╚═╝   ╚═╝      ╚═╝   
```       

MCP Unityは、Unityエディター向けのModel Context Protocolの実装であり、AIアシスタントがUnityプロジェクトと対話できるようにします。このパッケージは、UnityとMCPプロトコルを実装するNode.jsサーバー間のブリッジを提供し、Claude、Windsurf、CursorなどのAIエージェントがUnityエディター内で操作を実行できるようにします。

## 機能

<a href="https://glama.ai/mcp/servers/@CoderGamester/mcp-unity">
  <img width="400" height="200" src="https://glama.ai/mcp/servers/@CoderGamester/mcp-unity/badge" alt="Unity MCPサーバー" />
</a>

### IDE統合 - パッケージキャッシュアクセス

MCP Unityは、Unityの`Library/PackedCache`フォルダーをワークスペースに追加することで、VSCode系IDE（Visual Studio Code、Cursor、Windsurf）との自動統合を提供します。この機能により：

- Unityパッケージのコードインテリジェンスが向上
- Unityパッケージのより良いオートコンプリートと型情報が有効化
- AIコーディングアシスタントがプロジェクトの依存関係を理解するのに役立つ

### MCPサーバーツール

- `execute_menu_item`: Unityメニュー項目（MenuItem属性でタグ付けされた関数）を実行
  > **例:** "新しい空のGameObjectを作成するためにメニュー項目'GameObject/Create Empty'を実行"

- `select_gameobject`: パスまたはインスタンスIDでUnity階層内のゲームオブジェクトを選択
  > **例:** "シーン内のMain Cameraオブジェクトを選択"

- `update_component`: GameObject上のコンポーネントフィールドを更新、またはGameObjectに含まれていない場合は追加
  > **例:** "PlayerオブジェクトにRigidbodyコンポーネントを追加し、その質量を5に設定"

- `add_package`: Unityパッケージマネージャーに新しいパッケージをインストール
  > **例:** "プロジェクトにTextMeshProパッケージを追加"

- `run_tests`: Unityテストランナーを使用してテストを実行
  > **例:** "プロジェクト内のすべてのEditModeテストを実行"

- `send_console_log`: Unityにコンソールログを送信
  > **例:** "Unity Editorにコンソールログを送信"

- `add_asset_to_scene`: AssetDatabaseからアセットをUnityシーンに追加
  > **例:** "プロジェクトからPlayerプレハブを現在のシーンに追加"

### MCPサーバーリソース

- `unity://menu-items`: `execute_menu_item`ツールを容易にするために、Unityエディターで利用可能なすべてのメニュー項目のリストを取得
  > **例:** "GameObject作成に関連する利用可能なすべてのメニュー項目を表示"

- `unity://hierarchy`: Unity階層内のすべてのゲームオブジェクトのリストを取得
  > **例:** "現在のシーンの階層構造を表示"

- `unity://gameobject/{id}`: シーン階層内のインスタンスIDまたはオブジェクトパスで特定のGameObjectに関する詳細情報を取得
  > **例:** "Player GameObjectに関する詳細情報を取得"

- `unity://logs`: Unityコンソールからのすべてのログのリストを取得
  > **例:** "Unityコンソールからの最近のエラーメッセージを表示"

- `unity://packages`: Unityパッケージマネージャーからインストール済みおよび利用可能なパッケージに関する情報を取得
  > **例:** "Unityプロジェクトに現在インストールされているすべてのパッケージをリスト"

- `unity://assets`: Unityアセットデータベース内のアセットに関する情報を取得
  > **例:** "プロジェクト内のすべてのテクスチャアセットを検索"

- `unity://tests/{testMode}`: Unityテストランナー内のテストに関する情報を取得
  > **例:** "Unityプロジェクトで利用可能なすべてのテストをリスト"

## 要件
- Unity 2022.3以降 - [サーバーをインストール](#install-server)するため
- Node.js 18以降 - [サーバーを起動](#start-server)するため
- npm 9以降 - [サーバーをデバッグ](#debug-server)するため

## <a name="install-server"></a>インストール

このMCP Unityサーバーのインストールは複数ステップのプロセスです：

### ステップ1: Unityパッケージマネージャー経由でUnity MCPサーバーパッケージをインストール
1. Unityパッケージマネージャーを開く（Window > Package Manager）
2. 左上隅の"+"ボタンをクリック
3. "Add package from git URL..."を選択
4. 入力: `https://github.com/CoderGamester/mcp-unity.git`
5. "Add"をクリック

![package manager](https://github.com/user-attachments/assets/a72bfca4-ae52-48e7-a876-e99c701b0497)


### ステップ2: Node.jsをインストール
> MCP Unityサーバーを実行するには、コンピューターにNode.js 18以降がインストールされている必要があります：

<details>
<summary><span style="font-size: 1.1em; font-weight: bold;">Windows</span></summary>

1. [Node.jsダウンロードページ](https://nodejs.org/en/download/)にアクセス
2. LTSバージョンのWindowsインストーラー（.msi）をダウンロード（推奨）
3. インストーラーを実行し、インストールウィザードに従う
4. PowerShellを開いて以下を実行してインストールを確認：
   ```bash
   node --version
   ```
</details>

<details>
<summary><span style="font-size: 1.1em; font-weight: bold;">macOS</span></summary>

1. [Node.jsダウンロードページ](https://nodejs.org/en/download/)にアクセス
2. LTSバージョンのmacOSインストーラー（.pkg）をダウンロード（推奨）
3. インストーラーを実行し、インストールウィザードに従う
4. または、Homebrewがインストールされている場合は以下を実行：
   ```bash
   brew install node@18
   ```
5. ターミナルを開いて以下を実行してインストールを確認：
   ```bash
   node --version
   ```
</details>

### ステップ3: AI LLMクライアントを設定

<details open>
<summary><span style="font-size: 1.1em; font-weight: bold;">オプション1: Unityエディターを使用して設定</span></summary>

1. Unityエディターを開く
2. Tools > MCP Unity > Server Windowに移動
3. 以下の画像のようにAI LLMクライアントの"Configure"ボタンをクリック

![image](https://github.com/user-attachments/assets/8d286e83-da60-40fa-bd6c-5de9a77c1820)

4. 表示されるポップアップで設定インストールを確認

![image](https://github.com/user-attachments/assets/b1f05d33-3694-4256-a57b-8556005021ba)

</details>

<details>
<summary><span style="font-size: 1.1em; font-weight: bold;">オプション2: 手動設定</span></summary>

AIクライアントのMCP設定ファイル（例：Claude Desktopのclaude_desktop_config.json）を開き、以下のテキストをコピー：

> `ABSOLUTE/PATH/TO`をMCP Unityインストールの絶対パスに置き換えるか、UnityエディターMCPサーバーウィンドウ（Tools > MCP Unity > Server Window）からテキストをコピー

```json
{
  "mcpServers": {
    "mcp-unity": {
      "command": "node",
      "args": [
        "ABSOLUTE/PATH/TO/mcp-unity/Server/build/index.js"
      ]
    }
  }
}
```

</details>

## <a name="start-server"></a>サーバーの起動

MCP Unityサーバーを起動するには2つの方法があります：

### オプション1: Unityエディター経由で起動
1. Unityエディターを開く
2. Tools > MCP Unity > Server Windowに移動
3. "Start Server"ボタンをクリック

### オプション2: コマンドラインから起動
1. ターミナルまたはコマンドプロンプトを開く
2. MCP Unityサーバーディレクトリに移動
3. 以下のコマンドを実行：
   ```bash
   node Server/build/index.js
   ```

## オプション: タイムアウト設定

デフォルトでは、MCPサーバーとWebSocket間のタイムアウトは 10 秒です。
お使いのOSに応じて変更できます。

<details>
<summary><span style="font-size: 1.1em; font-weight: bold;">Option 1: Windows OS</span></summary>

1. Unityエディターを開きます
2. **Tools > MCP Unity > Server Window** に移動します
3. **Request Timeout (seconds)** の値を希望のタイムアウト秒数に変更します
4. Unityはシステム環境変数UNITY_REQUEST_TIMEOUTに新しいタイムアウト値を設定します
5. Node.jsサーバーを再起動します
6. **Start Server** をもう一度クリックして、UnityエディターのWebソケットをNode.js MCPサーバーに再接続します

</details>

<details>
<summary><span style="font-size: 1.1em; font-weight: bold;">Option 2: Windows以外のOS</span></summary>

Windows 以外の OS の場合は、次の 2 か所で設定する必要があります。

### エディター内プロセスのタイムアウト

1. Unityエディターを開きます
2. **Tools > MCP Unity > Server Window** に移動します
3. **Request Timeout (seconds)** の値を希望のタイムアウト秒数に変更します

### WebSocketのタイムアウト

1. ターミナルで UNITY_REQUEST_TIMEOUT 環境変数を設定します
    - Powershell
   ```powershell
   $env:UNITY_REQUEST_TIMEOUT = "300"
   ```
    - Command Prompt/Terminal
   ```cmd
   set UNITY_REQUEST_TIMEOUT=300
   ```
2. Node.jsサーバーを再起動します
3. **Start Server** をもう一度クリックして、UnityエディターのWebソケットをNode.js MCPサーバーに再接続します

</details>

> [!TIP]  
> AIコーディングIDE（Claude Desktop、Cursor IDE、Windsurf IDE など）とMCPサーバー間のタイムアウト設定は、IDEによって異なります。

## <a name="debug-server"></a>サーバーのデバッグ

MCP Unityサーバーをデバッグするには、以下の方法を使用できます：

### オプション1: Unityエディターを使用してデバッグ
1. Unityエディターを開く
2. Tools > MCP Unity > Server Windowに移動
3. "Debug Server"ボタンをクリック

### オプション2: コマンドラインを使用してデバッグ
1. ターミナルまたはコマンドプロンプトを開く
2. MCP Unityサーバーディレクトリに移動
3. 以下のコマンドを実行：
   ```bash
   npm run debug
   ```

## トラブルシューティング

<details>
<summary><span style="font-size: 1.1em; font-weight: bold;">接続の問題</span></summary>

- WebSocketサーバーが実行中であることを確認してください（UnityのServer Windowを確認）
- ファイアウォールの制限が接続を妨げていないか確認してください
- ポート番号が正しいことを確認してください（デフォルトは8080）
- UnityエディターのMCP Serverウィンドウでポート番号を変更できます（ツール > MCP Unity > Server Window）
</details>

<details>
<summary><span style="font-size: 1.1em; font-weight: bold;">サーバーが起動しない</span></summary>

- Unityコンソールにエラーメッセージがないか確認してください
- Node.jsが正しくインストールされ、PATHで利用可能であることを確認してください
- Serverディレクトリ内の依存関係がすべてインストールされていることを確認してください
</details>

<details>
<summary><span style="font-size: 1.1em; font-weight: bold;">Play Modeテスト実行時の接続失敗</span></summary>

`run_tests` ツールは以下の応答を返します：
```
Error:
Connection failed: Unknown error
```

このエラーは、Play Modeへ切り替える際にドメインリロードが発生し、ブリッジ接続が失われるために発生します。  
回避策は、**Edit > Project Settings > Editor > "Enter Play Mode Settings"** で **Reload Domain** をオフにすることです。
</details>

## サポート・フィードバック

ご質問やサポートが必要な場合は、このリポジトリの[issue](https://github.com/CoderGamester/mcp-unity/issues)をご利用ください。

また、以下でも連絡可能です：
- Linkedin: [![](https://img.shields.io/badge/LinkedIn-0077B5?style=flat&logo=linkedin&logoColor=white 'LinkedIn')](https://www.linkedin.com/in/miguel-tomas/)
- Discord: gamester7178
- メール: game.gamester@gmail.com

## コントリビューション

コントリビューションは大歓迎です！Pull Requestの送信やIssueの提出をお待ちしています。

**変更は [Conventional Commits](https://www.conventionalcommits.org/ja/v1.0.0/) フォーマットに従ってください。**

## ライセンス

本プロジェクトは [MIT License](License.md) の下で提供されています。

## 謝辞

- [Model Context Protocol](https://modelcontextprotocol.io)
- [Unity Technologies](https://unity.com)
- [Node.js](https://nodejs.org)
- [WebSocket-Sharp](https://github.com/sta/websocket-sharp)

## 貢献

貢献は大歓迎です！詳細については[貢献ガイド](CONTRIBUTING.md)をお読みください。

## ライセンス

このプロジェクトはMITライセンスの下でライセンスされています - 詳細は[LICENSE](LICENSE)ファイルを参照してください。

## 謝辞

- [Model Context Protocol](https://modelcontextprotocol.io)
- [Unity Technologies](https://unity.com)
- [Node.js](https://nodejs.org)
- [WebSocket-Sharp](https://github.com/sta/websocket-sharp)
