# TypeSafeInputSystem

Unity で利用可能な汎用性のある入力状態の取得パッケージです。  
Unity の Input System をラップし、ジェネリクスを活用した型安全で管理しやすい入力管理を提供します。

## 特徴

-   **型安全な入力管理**: ジェネリック型で入力アクションを識別
-   **柔軟な有効化制御**: グローバルと個別アクション単位での有効化/無効化
-   **マルチプレイヤー対応**: デバイス指定による複数プレイヤーの入力管理
-   **軽量・シンプル**: Unity Input System の薄いラッパー

## インストール

1. Unity Package Manager を開く。
2. 「Add package from git URL」を選択。
3. URL: `https://github.com/PipetteGames/TypeSafeInputSystem.git?path=Packages/TypeSafeInputSystem` を入力。
4. インストール完了。

または、Packages/manifest.json に以下を追加:

```json
{
    "dependencies": {
        "com.pipettegames.typesafeinputsystem": "https://github.com/PipetteGames/TypeSafeInputSystem.git?path=Packages/TypeSafeInputSystem"
    }
}
```

### 特定のバージョンを使用したい場合

URL の末尾にバージョン指定を追加

例: `https://github.com/PipetteGames/TypeSafeInputSystem.git?path=Packages/TypeSafeInputSystem#v0.3.1`

## 使い方

### 1. 入力アクションの定義

ジェネリック型 `T` として使用する列挙型を定義します：

```csharp
public enum InputActionType
{
    Move,
    Sprint,
    Interact,
    Pause
}
```

### 2. InputActionAsset の準備

Unity エディタで `InputActionAsset` を作成し、対応するアクションマップとアクションを定義します：

-   Action Map: `Player`
    -   Action: `Move` (Value Type: Vector2)
    -   Action: `Sprint` (Button)
    -   Action: `Interact` (Button)

### 3. TypedInputSystem の初期化

```csharp
using UnityEngine;
using UnityEngine.InputSystem;
using PipetteGames.TypeSafeInputSystem.Implements;
using PipetteGames.TypeSafeInputSystem.Interfaces;

public class PlayerController : MonoBehaviour
{
    private ITypedInputSystem<InputActionType> _typedInputSystem;

    private void Start()
    {
        // InputActionAsset を読み込み
        var inputActionAsset = Resources.Load<InputActionAsset>("InputSystem_Actions");

        // TypedInputSystem を初期化
        _typedInputSystem = new TypedInputSystem<InputActionType>(inputActionAsset);

        // アクションを登録
        _typedInputSystem.RegisterAction("Player", InputActionType.Move);
        _typedInputSystem.RegisterAction("Player", InputActionType.Sprint);
        _typedInputSystem.RegisterAction("Player", InputActionType.Interact);

        // 有効化
        _typedInputSystem.Enable();
    }

    private void Update()
    {
        // 入力の取得
        if (_typedInputSystem.IsPressed(InputActionType.Sprint))
        {
            Debug.Log("Sprint!");
        }

        if (_typedInputSystem.WasPressedThisFrame(InputActionType.Interact))
        {
            Debug.Log("Interact!");
        }

        // アナログ値の取得
        var moveInput = _typedInputSystem.ReadValue<Vector2>(InputActionType.Move);
        transform.Translate(moveInput * Time.deltaTime);
    }

    private void OnDestroy()
    {
        _typedInputSystem?.Dispose();
    }
}
```

### 4. 個別アクションの有効化/無効化

```csharp
// 特定のアクションのみ無効化
_typedInputSystem.Disable(InputActionType.Sprint);

// 特定のアクションのみ有効化
_typedInputSystem.Enable(InputActionType.Sprint);

// アクション単位での状態確認
if (_typedInputSystem.IsEnabled(InputActionType.Sprint))
{
    // Sprint が有効
}
```

### 5. 全体の有効化/無効化

```csharp
// 全体を無効化（すべてのアクションが無効になる）
_typedInputSystem.Disable();

// 全体を有効化
_typedInputSystem.Enable();

// 有効状態を確認
if (_typedInputSystem.IsEnabled())
{
    // 全体が有効
}
```

### 6. イベントドリブンな入力検知

コールバック方式で入力検知を行う場合：

```csharp
private void Start()
{
    // ... 初期化コード ...

    // Started イベント登録（入力開始時）
    _typedInputSystem.RegisterStarted(InputActionType.Sprint, OnSprintStarted);

    // Performed イベント登録（入力実行時）
    _typedInputSystem.RegisterPerformed(InputActionType.Interact, OnInteractPerformed);

    // Canceled イベント登録（入力終了時）
    _typedInputSystem.RegisterCanceled(InputActionType.Sprint, OnSprintCanceled);
}

private void OnSprintStarted(InputAction.CallbackContext context)
{
    Debug.Log("Sprint started!");
}

private void OnInteractPerformed(InputAction.CallbackContext context)
{
    Debug.Log("Interact performed!");
}

private void OnSprintCanceled(InputAction.CallbackContext context)
{
    Debug.Log("Sprint canceled!");
}

private void OnDestroy()
{
    // コールバックを登録解除
    _typedInputSystem?.UnregisterStarted(InputActionType.Sprint, OnSprintStarted);
    _typedInputSystem?.UnregisterPerformed(InputActionType.Interact, OnInteractPerformed);
    _typedInputSystem?.UnregisterCanceled(InputActionType.Sprint, OnSprintCanceled);

    _typedInputSystem?.Dispose();
}
```

**注意**: コールバック登録時と登録解除時には、**同じコールバック参照を使用する必要があります**。メソッドグループを直接渡さず、メソッド参照を保持して使用してください。

### 7. マルチプレイヤー対応

複数のデバイスを識別して、プレイヤーごとに入力を管理できます。

#### デバイス指定による入力管理

各プレイヤーに個別の `TypedInputSystem` インスタンスを割り当て、デバイスで識別します：

```csharp
using UnityEngine;
using UnityEngine.InputSystem;
using PipetteGames.TypeSafeInputSystem.Implements;
using PipetteGames.TypeSafeInputSystem.Interfaces;

public class MultiplayerManager : MonoBehaviour
{
    private ITypedInputSystem<InputActionType> _player1Input;
    private ITypedInputSystem<InputActionType> _player2Input;

    private void Start()
    {
        var inputActionAsset = Resources.Load<InputActionAsset>("InputSystem_Actions");

        // プレイヤー1: Gamepad1を使用
        var gamepad1 = Gamepad.all.Count > 0 ? Gamepad.all[0] : null;
        if (gamepad1 != null)
        {
            _player1Input = new TypedInputSystem<InputActionType>(inputActionAsset, gamepad1);
            _player1Input.RegisterAction("Player", InputActionType.Move);
            _player1Input.RegisterAction("Player", InputActionType.Sprint);
            _player1Input.Enable();
        }

        // プレイヤー2: Gamepad2を使用
        var gamepad2 = Gamepad.all.Count > 1 ? Gamepad.all[1] : null;
        if (gamepad2 != null)
        {
            _player2Input = new TypedInputSystem<InputActionType>(inputActionAsset, gamepad2);
            _player2Input.RegisterAction("Player", InputActionType.Move);
            _player2Input.RegisterAction("Player", InputActionType.Sprint);
            _player2Input.Enable();
        }
    }

    private void Update()
    {
        // プレイヤー1の入力
        if (_player1Input != null)
        {
            var player1Move = _player1Input.ReadValue<Vector2>(InputActionType.Move);
            Debug.Log($"Player1 Move: {player1Move}");
        }

        // プレイヤー2の入力
        if (_player2Input != null)
        {
            var player2Move = _player2Input.ReadValue<Vector2>(InputActionType.Move);
            Debug.Log($"Player2 Move: {player2Move}");
        }
    }

    private void OnDestroy()
    {
        _player1Input?.Dispose();
        _player2Input?.Dispose();
    }
}
```

#### 全プレイヤーの入力を統合管理

デバイスを指定せずにインスタンスを作成すると、すべてのデバイスからの入力を受け付けます：

```csharp
public class SingleInputManager : MonoBehaviour
{
    private ITypedInputSystem<InputActionType> _allPlayersInput;

    private void Start()
    {
        var inputActionAsset = Resources.Load<InputActionAsset>("InputSystem_Actions");

        // デバイス指定なし: 全デバイスからの入力を受け付ける
        _allPlayersInput = new TypedInputSystem<InputActionType>(inputActionAsset);
        _allPlayersInput.RegisterAction("Player", InputActionType.Pause);
        
        // コールバックを使用して、どのデバイスから入力があったかを判定
        _allPlayersInput.RegisterPerformed(InputActionType.Pause, OnPausePerformed);
        _allPlayersInput.Enable();
    }

    private void OnPausePerformed(InputAction.CallbackContext context)
    {
        // どのデバイスから入力があったかを確認
        var device = context.control.device;
        Debug.Log($"Pause pressed from device: {device.displayName}");
    }

    private void OnDestroy()
    {
        _allPlayersInput?.UnregisterPerformed(InputActionType.Pause, OnPausePerformed);
        _allPlayersInput?.Dispose();
    }
}
```

#### デバイスの取得

インスタンスに関連付けられているデバイスを確認できます：

```csharp
var device = _player1Input.Device;
if (device != null)
{
    Debug.Log($"Player 1 is using: {device.displayName}");
}
else
{
    Debug.Log("No specific device assigned (all devices)");
}
```

## API リファレンス

### プロパティ

| プロパティ | 説明                                                                                   |
| ---------- | -------------------------------------------------------------------------------------- |
| `Device`   | このインスタンスに関連付けられているデバイス（デバイス指定なしの場合は null を返す） |

### 登録

| メソッド                                                         | 説明                                               |
| ---------------------------------------------------------------- | -------------------------------------------------- |
| `RegisterAction(string actionMapName, T key)`                    | アクションを登録（アクション名はキーの名前を使用） |
| `RegisterAction(string actionMapName, T key, string actionName)` | アクションを登録（アクション名を指定）             |

### イベント登録

| メソッド                                              | 説明                                     |
| ----------------------------------------------------- | ---------------------------------------- |
| `RegisterStarted(T action, Action<InputAction.CallbackContext> callback)`    | 入力開始時のコールバックを登録           |
| `RegisterPerformed(T action, Action<InputAction.CallbackContext> callback)`  | 入力実行時のコールバックを登録           |
| `RegisterCanceled(T action, Action<InputAction.CallbackContext> callback)`   | 入力終了時のコールバックを登録           |
| `UnregisterStarted(T action, Action<InputAction.CallbackContext> callback)`    | 入力開始時のコールバックを登録解除       |
| `UnregisterPerformed(T action, Action<InputAction.CallbackContext> callback)`  | 入力実行時のコールバックを登録解除       |
| `UnregisterCanceled(T action, Action<InputAction.CallbackContext> callback)`   | 入力終了時のコールバックを登録解除       |

### 有効化制御

| メソッド            | 説明                     |
| ------------------- | ------------------------ |
| `Enable()`          | 全体を有効化             |
| `Disable()`         | 全体を無効化             |
| `Enable(T action)`  | 特定のアクションを有効化 |
| `Disable(T action)` | 特定のアクションを無効化 |

### 状態取得

| メソッド                         | 説明                                       |
| -------------------------------- | ------------------------------------------ |
| `IsEnabled()`                    | 全体が有効か                               |
| `IsEnabled(T action)`            | 特定のアクションが有効か                   |
| `IsPressed(T action)`            | 特定のアクションが押されているか           |
| `WasPressedThisFrame(T action)`  | 特定のアクションがこのフレームで押されたか |
| `WasReleasedThisFrame(T action)` | 特定のアクションがこのフレームで離されたか |
| `ReadValue<TValue>(T action)`    | 特定のアクションの値を取得（Vector2 など） |

## 設計概念

### グローバルフラグと個別フラグ

-   **グローバルフラグ (`Enable()`/`Disable()`)**: 全体のオン/オフを制御
-   **個別フラグ (`Enable(T action)`/`Disable(T action)`)**: 特定のアクションのみのオン/オフを制御

全体が無効な場合、すべてのアクション取得メソッド (`IsPressed`, `ReadValue` など) は無効な結果を返します。

### イベントドリブン入力検知

`RegisterStarted`, `RegisterPerformed`, `RegisterCanceled` メソッドを使用することで、イベント駆動型の入力処理が可能です。

-   **グローバルフラグの影響**: グローバルフラグが無効な場合、コールバックは呼び出されません。
-   **コールバック重複登録**: 同じコールバック参照を複数回登録しようとすると警告が出力され、無視されます。
-   **コールバック参照管理**: 登録解除時には、登録時と同じコールバック参照を使用する必要があります。

### マルチプレイヤー対応

マルチプレイヤーゲームでは、以下の2つのアプローチで入力を管理できます：

1. **プレイヤーごとにインスタンスを作成**: デバイスを指定して `TypedInputSystem` を初期化すると、そのデバイスからの入力のみを処理します（1プレイヤー = 1インスタンス）。
2. **全プレイヤー統合管理**: デバイスを指定せずに初期化すると、すべてのデバイスからの入力を受け付けます。コールバック内で `context.control.device` を使用して、どのデバイスから入力があったかを判定できます。

デバイスフィルタリングは、Unity Input System の薄いラッパーとして、最小限の実装で実現されています。

## ライセンス

MIT License
