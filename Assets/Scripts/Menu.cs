using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{

    public void LoadLevel1()
    {
        SceneManager.LoadScene("Project 4 - Level Easy");
    }

    public void LoadLevel2()
    {
        SceneManager.LoadScene("Project 4 - Level Medium");
    }

    public void LoadLevel3()
    {
        SceneManager.LoadScene("Project 4 - Level Hard");
    }

    public void LoadTitleScreen()
    {
        SceneManager.LoadScene("Project 4 - Title Screen");
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(SceneManager.GetActiveScene().name == "Project 4 - Title Screen")
        {
           if (Input.GetKeyDown(KeyCode.Escape))
            {
                bool isMenuActive = gameObject.activeSelf;
                gameObject.SetActive(!isMenuActive);
                if (!isMenuActive)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
                else
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
            }
    }
}
}