using System.Linq;
using UnityEngine;
using UnityEngine.UI; 
using System.Collections;
using System.Collections.Generic;

public class AudioShip : MonoBehaviour {
	
	// VARIABLES GLOBALES
	private AudioSource audioSource;
    private int FREQUENCY;// Frequence d'echantillonage
    private const int NB_SAMPLES = 512;   // Nombre d'échantillons contenus dans une trame
    private const float REFVALUE = 0.00045f;
    private const int NB_SAMPLES_MEAN = 4;
    private List<float> sampleBuffer;

    private float[] trame;
	
	// ===============================================
	// =========== METHODES START ET UPDATE ==========
	// ===============================================
	
	// Use this for initialization
	// ----------------------------
	void Start () {
        FREQUENCY = AudioSettings.outputSampleRate;

        trame = new float[NB_SAMPLES];

		this.audioSource = GetComponent<AudioSource>();
		StartMicListener();
        sampleBuffer = new List<float>();
    }

    // Update is called once per frame
    // -------------------------------
    void Update () {
		// If the audio has stopped playing, this will restart the mic play the clip.
		if (!audioSource.isPlaying) {
			StartMicListener();
        }

        votreFonction();
    }

    // ===============================================
    // ============== AUTRES METHODES ================
    // ===============================================

    // Starts the Mic, and plays the audio back in (near) real-time.
    // --------------------------------------------------------------
    private void StartMicListener() {
		if (audioSource.clip == null) {
			audioSource.clip = Microphone.Start ("Built-in Microphone", true, 999, FREQUENCY);
			// HACK - Forces the function to wait until the microphone has started, before moving onto the play function.
			while (!(Microphone.GetPosition("Built-in Microphone") > 0)) {
			} audioSource.Play ();
		}
	}
	
	// Votre Fonction
	// -------------------------------
	private void votreFonction(){
		audioSource.GetOutputData (trame, 0);

        float rms = ComputeRMSValue(trame);
        if(rms != 0f)
        {

            //Place the particules according to autocorelated trame
            float[] trameAutoCor = new float[NB_SAMPLES];
            trameAutoCor = GetAutoCorrelation(trame);

            //Make the line move if their is a period
            var localMax = LocalMaximas(trameAutoCor);

            localMax = localMax.OrderByDescending(e => e.Value).ToList();
            float pitch = 0.0f;

            if (localMax[1].Value > localMax[0].Value * 0.55f)
            {
                float period = Mathf.Abs(localMax[0].Key - localMax[1].Key);
                print(localMax[0].Value);
                pitch = (1 / period) * FREQUENCY;

                float pitchMapped = MapBetween(pitch, 0.0f, 1000.0f, 0.0f, 40.0f);
                transform.position += new Vector3(0.0f, pitch / , 0.0f);
            }


        }

    }

    private float ComputeRMSValue(float[] trame)
    {
        float total = 0.0f;
        for (int i = 0; i < NB_SAMPLES; ++i)
        {
            total += trame[i] * trame[i];
        }

        float mean = total / NB_SAMPLES;

        return Mathf.Sqrt(mean);
    }

    private float ComputeDecibelLevel(float rms)
    {
        return 20.0f * Mathf.Log10(rms / REFVALUE);
    }

    private float ComputeMean(List<float> values)
    {
        float mean = 0.0f;
        for (int i = 0; i < values.Count; ++i)
        {
            mean += values[i];
        }

        return mean / values.Count;
    }

    float MapBetween(float value, float valueMin, float valueMax, float resultMin, float resultMax)
    {
        return (resultMax - resultMin) / (valueMax - valueMin) * (value - valueMax) + resultMax;
    }

    float[] GetAutoCorrelation(float[] sample)
    {
        float[] result = new float[NB_SAMPLES];
        float sum;
        for(int l = 0; l < NB_SAMPLES; ++l)
        {
            sum = 0.0f;
            for(int k = 0; k < NB_SAMPLES; ++k)
            {
                sum += sample[k] * (k - l < 0 ? 0.0f : sample[k - l]);
            }
            result[l] = sum;
        }

        return result;
    }

    List<KeyValuePair<int, float>> LocalMaximas(float[] values)
    {
        List<KeyValuePair<int, float>> maxima = new List<KeyValuePair<int, float>>();

        for(int i = 0; i < values.Length - 1; ++i)
        {
            if(i == 0)
            {
                if(values[i + 1] < values[i])
                {
                    maxima.Add(new KeyValuePair<int, float>(i, values[i]));
                }
            }
            else
            {
                if(values[i - 1] <= values[i] && values[i + 1] <= values[i])
                {
                    maxima.Add(new KeyValuePair<int, float>(i, values[i]));
                }
            }
        }

        return maxima;
    }
}
