using Firebase;
using Firebase.Auth;
using Firebase.Database;
using System;
using System.Collections.Generic;
using System.Security;
using System.Threading.Tasks;
using UnityEngine;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance { get; private set; }

    public bool IsReady { get; private set; }

    private readonly System.Threading.Tasks.TaskCompletionSource<bool> readyTcs
        = new System.Threading.Tasks.TaskCompletionSource<bool>();

    public System.Threading.Tasks.Task ReadyTask => readyTcs.Task;

    public bool IsSignedIn => auth != null && auth.CurrentUser != null;
    public string UserId => auth?.CurrentUser?.UserId;
    public string Email => auth?.CurrentUser?.Email;

    private FirebaseAuth auth;
    private DatabaseReference db;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private async void Start()
    {
        await InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        var status = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (status != DependencyStatus.Available)
        {
            Debug.LogError($"[Firebase] 의존성 해결 실패: {status}");
            readyTcs.SetResult(false);
            return;
        }

        var app = FirebaseApp.DefaultInstance;
        auth = FirebaseAuth.GetAuth(app);
        db = FirebaseDatabase.DefaultInstance.RootReference;
        IsReady = true;
        Debug.Log("[Firebase] 초기화 완료");
        readyTcs.TrySetResult(true);
    }

    public async Task<(bool ok, string error)> SignUpAsync(string email, string password)
    {
        if (!IsReady) return (false, "Firebase 초기화 중입니다. 잠시 후 다시 시도하세요.");
        try
        {
            var result = await auth.CreateUserWithEmailAndPasswordAsync(email.Trim(), password);
            Debug.Log($"[Firebase] 회원가입 성공: {result.User.Email}");
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, ParseAuthError(ex));
        }
    }

    public async Task<(bool ok, string error)> SignInAsync(string email, string password)
    {
        if (!IsReady) return (false, "Firebase 초기화 중입니다. 잠시 후 다시 시도하세요.");
        try
        {
            var result = await auth.SignInWithEmailAndPasswordAsync(email.Trim(), password);
            Debug.Log($"[Firebase] 로그인 성공: {result.User.Email}");
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, ParseAuthError(ex));
        }
    }

    public void SignOut()
    {
        auth?.SignOut();
        Debug.Log("[Firebase] 로그아웃");
    }

    // 세이브
    public async Task<bool> SaveGameAsync(GameSaveData data)
    {
        if (!IsReady || !IsSignedIn || data == null) return false;
        try
        {
            string json = JsonUtility.ToJson(data);
            var payload = new Dictionary<string, object>
            {
                ["save"] = json,
                ["currentDay"] = data.currentDay,
                ["updatedAt"] = ServerValue.Timestamp,
            };
            await db.Child("users").Child(UserId).UpdateChildrenAsync(payload);
            Debug.Log("[Firebase] 클라우드 저장 완료");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Firebase] 클라우드 저장 실패: {ex.Message}");
            return false;
        }

    }

    public async Task<GameSaveData> LoadGameAsync()
    {
        if (!IsReady || !IsSignedIn) return null;
        try
        {
            var snap = await db.Child("users").Child(UserId).Child("save").GetValueAsync();
            if (snap == null || !snap.Exists) return null;
            string json = snap.Value as string;
            if (string.IsNullOrEmpty(json)) return null;
            return JsonUtility.FromJson<GameSaveData>(json);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Firebase] 클라우드 로드 실패ㅣ {ex.Message}");
            return null;
        }
    }

    private static string ParseAuthError (Exception ex)
    {
        var fe = ex as FirebaseException
                ?? (ex.InnerException as FirebaseException)
                ?? (ex as AggregateException)?.Flatten().InnerException as FirebaseException;

        if (fe == null) return $"알 수 없는 오류: {ex.Message}";

        switch ((AuthError)fe.ErrorCode)
        {
            case AuthError.InvalidEmail: return "이메일 형식이 올바르지 않습니다.";
            case AuthError.MissingEmail: return "이메일을 입력하세요.";
            case AuthError.MissingPassword: return "비밀번호를 입력하세요.";
            case AuthError.WeakPassword: return "비밀번호는 6자 이상이어야 합니다.";
            case AuthError.EmailAlreadyInUse: return "이미 가입된 이메일입니다.";
            case AuthError.WrongPassword: return "비밀번호가 틀렸습니다.";
            case AuthError.UserNotFound: return "가입되지 않은 이메일입니다.";
            case AuthError.NetworkRequestFailed: return "네트워크 연결을 확인하세요.";
            case AuthError.TooManyRequests: return "시도가 너무 많습니다. 잠시 후 다시 시도하세요.";

            default: 
                Debug.LogError($"[Auth] code ={fe.ErrorCode}, msg={fe.Message}");
                return $"인증 오류: {fe.Message}";
        }
    }

}
