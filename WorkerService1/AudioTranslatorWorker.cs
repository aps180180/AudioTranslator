using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.Translation;
using Microsoft.Extensions.Logging;

namespace AudioTranslatorWorker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly string _audioFilePath = "audio.wav";
        private readonly string _subscriptionKey = "";
        private readonly string _region = "brazilsouth";
        private readonly string _sourceLanguage = "pt-BR"; 
        private readonly string _targetLanguage = "en"; 

        private SpeechRecognizer _speechRecognizer;
        private TranslationRecognizer _translationRecognizer;
        private SpeechSynthesizer _speechSynthesizer;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {

            _logger.LogInformation("Service is starting.");
            // Initialize the audio capture device
            var audioConfig = AudioConfig.FromDefaultMicrophoneInput();

            // Initialize the speech recognizer
            var speechConfig = SpeechConfig.FromSubscription(_subscriptionKey, _region);
            speechConfig.SpeechRecognitionLanguage = _sourceLanguage;
            _speechRecognizer = new SpeechRecognizer(speechConfig, audioConfig);

            // Initialize the translation recognizer
            var translationConfig = SpeechTranslationConfig.FromSubscription(_subscriptionKey, _region);
            translationConfig.SpeechRecognitionLanguage = _sourceLanguage;
            translationConfig.AddTargetLanguage(_targetLanguage);
            _translationRecognizer = new TranslationRecognizer(translationConfig, audioConfig);

            // Initialize the speech synthesizer
            var synthesizerConfig = SpeechConfig.FromSubscription(_subscriptionKey, _region);
            synthesizerConfig.SpeechSynthesisLanguage = _targetLanguage;
            _speechSynthesizer = new SpeechSynthesizer(synthesizerConfig);

            _speechRecognizer.Recognizing += SpeechRecognizer_Recognizing;
            _translationRecognizer.Recognized += TranslationRecognizer_Recognized;

            await _speechRecognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);
            await _translationRecognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);



            await base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Service is running.");

            while (!stoppingToken.IsCancellationRequested)
            {
                // _logger.LogInformation("Service is working...");
                await Task.Delay(1000, stoppingToken);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Service is stopping.");

            await _speechRecognizer.StopContinuousRecognitionAsync();
            await _translationRecognizer.StopContinuousRecognitionAsync();

            _speechRecognizer.Dispose();
            _translationRecognizer.Dispose();
            _speechSynthesizer.Dispose();

            await base.StopAsync(cancellationToken);
        }

        private void SpeechRecognizer_Recognizing(object sender, SpeechRecognitionEventArgs e)
        {
            // Handle audio data (e.g., log or process it)
            // _logger.LogInformation($"Recognized: {e.Result.Text}");
        }

        private async void TranslationRecognizer_Recognized(object sender, TranslationRecognitionEventArgs e)
        {
            if (e.Result.Reason == ResultReason.TranslatedSpeech)
            {
                if (e.Result.Translations.ContainsKey(_targetLanguage))
                {
                    var translatedText = e.Result.Translations[_targetLanguage];
                    var originalText = e.Result.Text;
                    var output =
                                 $"Original Text: {originalText}\n" +
                                 $"Translated Text: {translatedText}\n" +
                                 $"----------------------------";

                    _logger.LogInformation(output);
                }

                //  using var result = await _speechSynthesizer.SpeakTextAsync(output).ConfigureAwait(false);
                // Handle or save the synthesized audio if needed
            }
        }
    }
}
