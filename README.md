# TypeSafeInputSystem

Unity で利用可能な汎用性のある入力状態の取得パッケージです。  
Unity の Input System をラップし、ジェネリクスを活用した型安全で管理しやすい入力管理を提供します。

## 特徴

-   **型安全な入力管理**: ジェネリック型で入力アクションを識別
-   **柔軟な有効化制御**: グローバルと個別アクション単位での有効化/無効化
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

例: `https://github.com/PipetteGames/TypeSafeInputSystem.git?path=Packages/TypeSafeInputSystem#v0.4.0`

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

    // Started イベント購読（入力開始時）
    _typedInputSystem.SubscribeStarted(InputActionType.Sprint, OnSprintStarted);

    // Performed イベント購読（入力実行時）
    _typedInputSystem.SubscribePerformed(InputActionType.Interact, OnInteractPerformed);

    // Canceled イベント購読（入力終了時）
    _typedInputSystem.SubscribeCanceled(InputActionType.Sprint, OnSprintCanceled);
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
    // イベントを購読解除
    _typedInputSystem?.UnsubscribeStarted(InputActionType.Sprint, OnSprintStarted);
    _typedInputSystem?.UnsubscribePerformed(InputActionType.Interact, OnInteractPerformed);
    _typedInputSystem?.UnsubscribeCanceled(InputActionType.Sprint, OnSprintCanceled);

    _typedInputSystem?.Dispose();
}
```

**注意**: コールバック購読時と購読解除時には、**同じコールバック参照を使用する必要があります**。メソッドグループを直接渡さず、メソッド参照を保持して使用してください。

### 7. IInputSubscription による購読管理

`Subscribe*` メソッドは `IInputSubscription` を返すため、`Dispose()` を使用して購読解除することもできます：

```csharp
public class PlayerController : MonoBehaviour
{
    private ITypedInputSystem<InputActionType> _typedInputSystem;
    private IInputSubscription _sprintSubscription;

    private void Start()
    {
        // ... 初期化コード ...

        // 購読を変数で保持
        _sprintSubscription = _typedInputSystem.SubscribeStarted(InputActionType.Sprint, OnSprintStarted);
    }

    private void OnSprintStarted(InputAction.CallbackContext context)
    {
        Debug.Log("Sprint started!");
    }

    private void OnDestroy()
    {
        // Dispose() で購読解除
        _sprintSubscription?.Dispose();
        _typedInputSystem?.Dispose();
    }
}
```

`using` 文を使用すると、スコープを抜けた際に自動的に購読解除されます：

```csharp
private void TemporarySubscribe()
{
    // using 文でスコープ内のみ購読
    using (var subscription = _typedInputSystem.SubscribePerformed(InputActionType.Interact, OnInteract))
    {
        // このスコープ内でのみコールバックが有効
        DoSomething();
    } // スコープを抜けると自動的に購読解除
}

private void OnInteract(InputAction.CallbackContext context)
{
    Debug.Log("Interact!");
}
```

## API リファレンス

### 登録

| メソッド                                                         | 説明                                               |
| ---------------------------------------------------------------- | -------------------------------------------------- |
| `RegisterAction(string actionMapName, T key)`                    | アクションを登録（アクション名はキーの名前を使用） |
| `RegisterAction(string actionMapName, T key, string actionName)` | アクションを登録（アクション名を指定）             |

### イベント購読

| メソッド                                              | 説明                                     |
| ----------------------------------------------------- | ---------------------------------------- |
| `SubscribeStarted(T action, Action<InputAction.CallbackContext> callback)`    | 入力開始時のイベントを購読。IInputSubscriptionを返す           |
| `SubscribePerformed(T action, Action<InputAction.CallbackContext> callback)`  | 入力実行時のイベントを購読。IInputSubscriptionを返す           |
| `SubscribeCanceled(T action, Action<InputAction.CallbackContext> callback)`   | 入力終了時のイベントを購読。IInputSubscriptionを返す           |
| `UnsubscribeStarted(T action, Action<InputAction.CallbackContext> callback)`    | 入力開始時のイベントを購読解除       |
| `UnsubscribePerformed(T action, Action<InputAction.CallbackContext> callback)`  | 入力実行時のイベントを購読解除       |
| `UnsubscribeCanceled(T action, Action<InputAction.CallbackContext> callback)`   | 入力終了時のイベントを購読解除       |

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

`SubscribeStarted`, `SubscribePerformed`, `SubscribeCanceled` メソッドを使用することで、イベント駆動型の入力処理が可能です。

-   **グローバルフラグの影響**: グローバルフラグが無効な場合、コールバックは呼び出されません。
-   **コールバック重複購読**: 同じコールバック参照を複数回購読しようとすると警告が出力され、無視されます。
-   **コールバック参照管理**: 購読解除時には、購読時と同じコールバック参照を使用する必要があります。
-   **IInputSubscription**: 各 Subscribe メソッドは `IInputSubscription` を返すため、`Dispose()` で購読解除が可能です。

## ライセンス

MIT License
