using System;
using System.Linq;
using System.Collections;
using System.Net.Mime;
using TMPro;
using Unity.Collections;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class AuthManager : MonoBehaviour
{

    [SerializeField] string url = "https://sid-restapi.onrender.com";


    string Token;
    public string Username;
    public TMP_Text leaders;

    public TMP_Text errorMess;
    public GameObject Leaderboard;

    void Start()
    {
        Token = PlayerPrefs.GetString("token");
        Username = PlayerPrefs.GetString("username");

        if (string.IsNullOrEmpty(Token) || string.IsNullOrEmpty(Username))
        {
            Debug.Log("No hay token");
        }
        else
        {
            StartCoroutine(GetPerfil());
        }
    }

    public void Login()
    {
        Credentials credentials = new Credentials();

        credentials.username = GameObject.Find("InputFieldUsername").GetComponent<TMP_InputField>().text;
        credentials.password = GameObject.Find("InputFieldPassword").GetComponent<TMP_InputField>().text;

        string postData = JsonUtility.ToJson(credentials);

        StartCoroutine(LoginPost(postData));
    }

    public void Register()
    {
        Credentials credentials = new Credentials();

        credentials.username = GameObject.Find("InputFieldUsername").GetComponent<TMP_InputField>().text;
        credentials.password = GameObject.Find("InputFieldPassword").GetComponent<TMP_InputField>().text;

        string postData = JsonUtility.ToJson(credentials);

        StartCoroutine(RegisterPost(postData));
    }

    public void LogOut()
    {
        StartCoroutine("Logout");
    }

    IEnumerator Logout()
    {
        PlayerPrefs.DeleteKey("token");
        PlayerPrefs.DeleteKey("username");

        SceneManager.LoadScene("Menu");

        Debug.Log("Sesión cerrada localmente");
        yield return null;
    }

    public void GetLeaderboard()
    {
        if(Leaderboard.activeSelf)
        {
            Leaderboard.SetActive(false);
        }
        else
        {
            Leaderboard.SetActive(true);
            StartCoroutine("GetUsers");
        }
    }

    IEnumerator RegisterPost(string postData)
    {
        string path = "/api/usuarios";
        UnityWebRequest www = UnityWebRequest.Put(url + path, postData);
        www.method = "POST";
        www.SetRequestHeader("Content-Type", "application/json");
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(www.error);
        }
        else
        {
            if(www.responseCode == 200)
            {
                Debug.Log(www.downloadHandler.text);
                StartCoroutine(LoginPost(postData));
            }
            else
            {
                ErrorResponse error = JsonUtility.FromJson<ErrorResponse>(www.downloadHandler.text);

                switch (error.msg)
                {
                    case "Debe enviar el usuario":
                        errorMess.text = "Falta ingresar el usuario.";
                        break;

                    case "Debe enviar el password":
                        errorMess.text = "Falta ingresar la contraseña";
                        break;

                    case "Ya existe usuario con ese username":
                        errorMess.text = "El usuario ya está registrado.";
                        break;
                }
            }
        }
    }

    IEnumerator LoginPost(string postData)
    {
        string path = "/api/auth/login";
        UnityWebRequest www = UnityWebRequest.Put(url + path, postData);
        www.method = "POST";
        www.SetRequestHeader("Content-Type", "application/json");
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(www.error);
        }
        else
        {
            if(www.responseCode == 200)
            {
                string json = www.downloadHandler.text;

                AuthResponse response = JsonUtility.FromJson<AuthResponse>(json);

                PlayerPrefs.SetString("token",response.token);
                PlayerPrefs.SetString("username",response.usuario.username);
                if (SceneManager.GetActiveScene().name != "ClickerGame")
                {
                    SceneManager.LoadScene("ClickerGame");
                    
                }
            }
            else
            {
                string mensaje = "status: " + www.responseCode;
                mensaje += "\nError: " + www.downloadHandler.text;
                Debug.Log(mensaje);
                ErrorResponse error = JsonUtility.FromJson<ErrorResponse>(www.downloadHandler.text);

                switch (error.msg)
                {
                    case "debe enviar el campo password en la petición":
                        errorMess.text = "Falta ingresar la contraseña.";
                        break;

                    case "Usuario o contraseña no son correctos - correo":
                        errorMess.text = "Usuario o contraseña incorrectos.";
                        break;

                    case "debe enviar el campo username en la petición":
                        errorMess.text = "Falta ingresar el usuario.";
                        break;
                }
            }
        }
    }

    IEnumerator GetPerfil()
    {
        Debug.Log("Aqui si");
        string path = "/api/usuarios/" + Username;
        UnityWebRequest www = UnityWebRequest.Get(url + path);
        www.SetRequestHeader("x-token", Token);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(www.error);
        }
        else
        {
            if(www.responseCode == 200)
            {
                string json = www.downloadHandler.text;
                AuthResponse response = JsonUtility.FromJson<AuthResponse>(json);
                if (SceneManager.GetActiveScene().name != "ClickerGame")
                {
                    SceneManager.LoadScene("ClickerGame");
                }
            }
            else
            {
                Debug.Log("Token vencido... Redireccionar a Login");
            }
        }
    }

    IEnumerator GetUsers()
    {
        Debug.Log("pare");
        string path = "https://sid-restapi.onrender.com/api/usuarios";
        UnityWebRequest www = UnityWebRequest.Get(path);
        www.SetRequestHeader("x-token", Token);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(www.error);
        }
        else
        {
            if(www.responseCode == 200)
            {
                string json = www.downloadHandler.text;
                UsersList response = JsonUtility.FromJson<UsersList>(json);

                UserModel[] leaderboard = response.usuarios.OrderByDescending(u => u.data.score).Take(3).ToArray();
                leaders.text = "";
                foreach (var user in leaderboard)
                {
                    leaders.text += user.username + " | " + user.data.score + "\n\n";
                    Debug.Log(leaders.text);
                }
            }
            else
            {
                Debug.Log(www.responseCode);
            }
        }
    }

    public void PatchUsuario(int newScore)
{
    StartCoroutine(PatchUsuarioCoroutine(newScore));
    StartCoroutine("GetUsers");
}

IEnumerator PatchUsuarioCoroutine(int newScore)
{
    if (string.IsNullOrEmpty(Token) || string.IsNullOrEmpty(Username))
    {
        Debug.Log("No hay token o username, no se puede hacer PATCH.");
        yield break;
    }

    string endpoint = url + "/api/usuarios"; 

    UserModel body = new UserModel();
    body.username = Username;
    body.data = new DataUser();
    body.data.score = newScore;

    string jsonBody = JsonUtility.ToJson(body);

    UnityWebRequest request = UnityWebRequest.Put(endpoint, jsonBody);
    request.method = "PATCH";  
    request.SetRequestHeader("Content-Type", "application/json");
    request.SetRequestHeader("x-token", Token);

    yield return request.SendWebRequest();

    if (request.result == UnityWebRequest.Result.ConnectionError ||
        request.result == UnityWebRequest.Result.ProtocolError)
    {
        Debug.Log("Error al hacer PATCH: " + request.error);
        Debug.Log("Detalle: " + request.downloadHandler.text);
    }
    else
    {
        if (request.responseCode == 200)
        {
            Debug.Log("Score actualizado correctamente con PATCH!");
            StartCoroutine(GetUsers());
        }
        else
        {
            Debug.Log("Error al actualizar Score. Respuesta: " + request.downloadHandler.text);
        }
    }
}

}

public class Credentials
{
    public string username;
    public string password;
}

[System.Serializable]
public class AuthResponse
{
    public UserModel usuario;
    public string token;
}

[System.Serializable]
public class UserModel
{
    public string _id;
    public string username;
    public bool estado;
    public DataUser data;
}

[System.Serializable]
public class UsersList
{
    public UserModel[] usuarios;
}

[System.Serializable]
public class DataUser
{
    public int score;
}

[System.Serializable]
public class ErrorResponse
{
    public string msg;
}