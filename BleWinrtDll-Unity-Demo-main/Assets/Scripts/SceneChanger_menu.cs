using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger_menu : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void cambiar_escena_menu()
    {
        SceneManager.LoadScene("Menu");
    }
    public void cambiar_escena_nimble_menu()
    {
        SceneManager.LoadScene("nimble_menu");
    }
    public void cambiar_escena_nimble_connect()
    {
        SceneManager.LoadScene("nimble_connect");
    }
    public void cambiar_escena_nimble_FileName()
    {
        SceneManager.LoadScene("nimble_FileName");
    }
    public void cambiar_escena_nimble_REC()
    {
        SceneManager.LoadScene("nimble_REC");
    }
}
