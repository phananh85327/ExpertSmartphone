import sys
import json
import pandas as pd
import pickle
import numpy as np
from sklearn.preprocessing import StandardScaler

def read_input():
    try:
        input_data = sys.stdin.readline()
        return json.loads(input_data)
    except Exception as e:
        print(f"Error reading input: {str(e)}", file=sys.stderr)
        sys.exit(1)

def main():
    input_json = read_input()
    model_path = 'kmeans_model.pkl'
    new_data_path = input_json.get("NewDataPath")

    try:
        with open(model_path, "rb") as f:
            model_bundle = pickle.load(f)

        kmeans = model_bundle["model"]
        scaler = model_bundle["scaler"]
        fields = model_bundle["fields"]

        df_new = pd.read_csv(new_data_path)

        if not all(field in df_new.columns for field in fields):
            missing = [field for field in fields if field not in df_new.columns]
            raise ValueError(f"Missing required fields: {missing}")

        data = df_new[fields].fillna(0)
        scaled_data = scaler.transform(data)
        cluster_labels = kmeans.predict(scaled_data)

        output = pd.DataFrame({
            "ProductID": df_new["ProductID"],
            "Cluster": cluster_labels
        })

        print(output.to_json(orient="records"))

    except Exception as e:
        print(f"Error during prediction: {str(e)}", file=sys.stderr)
        sys.exit(1)

if __name__ == '__main__':
    main()
