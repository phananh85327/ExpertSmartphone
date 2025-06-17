import sys
import json
import pandas as pd
import pickle
import numpy as np
from sklearn.preprocessing import StandardScaler
 
def read_input_from_stdin():
    try:
        # Read entire input from standard input
        input_data = sys.stdin.read()
        if not input_data:
            raise Exception("No input data provided.")
        return json.loads(input_data)
    except Exception as e:
        print(f"Error reading input: {str(e)}", file=sys.stderr)
        sys.exit(1)
 
def main():
    # Read new product data from standard input; expecting a JSON object.
    new_product_data = read_input_from_stdin()  # e.g. {"Memory": 4, "Storage": 64, "Rating": 4.30, ...}
    model_path = 'kmeans_model.pkl'
    try:
        # Load the model bundle which contains the k-means model, scaler, and expected fields.
        with open(model_path, "rb") as f:
            model_bundle = pickle.load(f)
        kmeans = model_bundle["model"]
        scaler = model_bundle["scaler"]
        fields = model_bundle["fields"]
 
        # Convert new_product_data (a dictionary) into a DataFrame with one row.
        df_new = pd.DataFrame([new_product_data])
        # Validate that all required fields are present.
        if not all(field in df_new.columns for field in fields):
            missing = [field for field in fields if field not in df_new.columns]
            raise ValueError(f"Missing required fields: {missing}")
 
        // Fill any missing values with a default value (here using 0).
        data = df_new[fields].fillna(0)
        scaled_data = scaler.transform(data)
        # Predict cluster assignment.
        cluster_labels = kmeans.predict(scaled_data)
        assigned_cluster = int(cluster_labels[0])
        # Output the assigned cluster (as a plain string).
        print(assigned_cluster)
    except Exception as e:
        print(f"Error during prediction: {str(e)}", file=sys.stderr)
        sys.exit(1)
 
if __name__ == '__main__':
    main()