import sys
import json
import pickle
import numpy as np
import os

def read_input():
    try:
        input_data = sys.stdin.readline()
        return json.loads(input_data)
    except Exception as e:
        print(f"Error reading input: {str(e)}", file=sys.stderr)
        sys.exit(1)

def load_model(model_path):
    if not os.path.exists(model_path):
        print(f"Model file not found at {model_path}", file=sys.stderr)
        sys.exit(1)
    with open(model_path, 'rb') as f:
        return pickle.load(f)

def assign_cluster(model_data, input_features):
    model = model_data['model']
    scaler = model_data['scaler']
    fields = model_data['fields']

    try:
        feature_vector = [input_features[field] for field in fields]
        scaled_vector = scaler.transform([feature_vector])
        cluster = model.predict(scaled_vector)[0]
        print(cluster)
    except Exception as e:
        print(f"Error assigning cluster: {str(e)}", file=sys.stderr)
        sys.exit(1)

def main():
    input_json = read_input()
    model_path = "kmeans_model.pkl"
    model_data = load_model(model_path)
    assign_cluster(model_data, input_json)

if __name__ == '__main__':
    main()