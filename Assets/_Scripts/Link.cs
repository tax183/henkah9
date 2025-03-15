using UnityEngine;
using UnityEngine.SceneManagement;

public class Link : MonoBehaviour
{
    //frist
    public void GoToSecondPage()
    {
        SceneManager.LoadScene("second");
    }

    public void GoToGamesPageFirst()
    {
        SceneManager.LoadScene("Games page");
    }


    //second
    public void GoToThirdPage()
    {
        SceneManager.LoadScene("Third");
    }
    public void GoToGamesPageSecond()
    {
        SceneManager.LoadScene("Games page");
    }

    public void GoToFristPageSecond()
    {
        SceneManager.LoadScene("frist");
    }


    //Third
    public void GoToFourthPage()
    {
        SceneManager.LoadScene("Fourth");
    }
    public void GoToGamesPageThird()
    {
        SceneManager.LoadScene("Games page");
    }

    public void GoToScondPageThird()
    {
        SceneManager.LoadScene("second");
    }

    //Fourth
    public void GoToUm9MenuPage()
    {
        SceneManager.LoadScene("um9 menu page");
    }
    public void GoToGamesPageFourth()
    {
        SceneManager.LoadScene("Games page");
    }

    public void GoToThirdPageFourth()
    {
        SceneManager.LoadScene("Third");
    }




}
