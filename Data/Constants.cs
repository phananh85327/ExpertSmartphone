namespace Backend.Data
{
    public class Constants
    {
        public const int BATCH_SIZE = 1000;
        public const bool CREATE_PROLOG = true;
        public const bool CREATE_CLUSTER = true;
        public const bool CREATE_DATASET = false;
        public const string PROLOG_FILE_NAME = "expert.pl";
        public const string CLUSTER_FILE_NAME = "cluster.csv";
        public const string DATASET_FILE_NAME = "dataset.csv";
        public const string PYTHON_VENV = "Python/.venv/Scripts/python.exe";
        public const string PYTHON_KMEANS_SCRIPT_FILE_PATH = "Python/main.py";
    }
}
