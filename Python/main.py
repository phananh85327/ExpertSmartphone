import sys
import json
import pandas as pd
from sklearn.cluster import KMeans
from sklearn.preprocessing import StandardScaler
from sklearn.metrics import silhouette_score
import numpy as np
import pickle
import os

def read_input():
    try:
        input_data = sys.stdin.readline()
        return json.loads(input_data)
    except Exception as e:
        print(f"Error reading input: {str(e)}", file=sys.stderr)
        sys.exit(1)

def choose_k_elbow_method(data, max_k=10):
    distortions = []
    K = range(2, max_k + 1)
    for k in K:
        kmeans = KMeans(n_clusters=k, random_state=42, n_init=10)
        kmeans.fit(data)
        distortions.append(kmeans.inertia_)

    deltas = np.diff(distortions)
    second_deltas = np.diff(deltas)
    elbow_k = K[np.argmin(second_deltas) + 1] if len(second_deltas) > 0 else 2
    return elbow_k

def main():
    input_json = read_input()
    file_path = input_json.get("FilePath")
    fields = input_json.get("Fields")
    k = input_json.get("K")
    model_out_path = input_json.get("ModelOutputPath", "kmeans_model.pkl")

    # file_path = r"D:\\00 - Project\\ExpertSmartphone\\Backend\\dataset.csv"
    # fields = ["Memory", "Storage", "Rating", "OriginalPrice", "DiscountPercentage", "SellersAmount", "ScreenSize", "BatterySize", "Reviews"]
    # k = -1
    # model_out_path = input_json.get("ModelOutputPath", "kmeans_model.pkl")

    try:
        df = pd.read_csv(file_path)
        data = df[fields].dropna()

        scaler = StandardScaler()
        scaled_data = scaler.fit_transform(data)

        if k is None or k <= 0:
            k = choose_k_elbow_method(scaled_data, max_k=10)

        kmeans = KMeans(n_clusters=k, random_state=42, n_init=10)
        kmeans.fit(scaled_data)

        # Save model and scaler
        with open(model_out_path, "wb") as f:
            pickle.dump({
                "model": kmeans,
                "scaler": scaler,
                "fields": fields
            }, f)

        cluster_labels = [-1] * len(df)
        valid_indices = data.index.tolist()
        for idx, label in zip(valid_indices, kmeans.labels_):
            cluster_labels[idx] = int(label)

        print(json.dumps(cluster_labels))

    except Exception as e:
        print(f"Error during clustering: {str(e)}", file=sys.stderr)
        sys.exit(1)

if __name__ == '__main__':
    main()
