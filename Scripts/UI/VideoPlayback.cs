using UnityEngine;
using UnityEngine.Video;

public class VideoPlayback : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    

    void Start()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        videoPlayer.time = 0f;
        videoPlayer.Play(); // Start playing the video
    }
    public void ResetVideo()
    {
        // Stop the video and reset its playback time to the beginning
        videoPlayer = GetComponent<VideoPlayer>();
        videoPlayer.Stop();
        videoPlayer.time = 0f;
        videoPlayer.Play();
    }
}