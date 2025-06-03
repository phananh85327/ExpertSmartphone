#!/usr/bin/env python3
import argparse
import json
import pandas as pd
import numpy as np
from sklearn.cluster import KMeans
from sklearn.preprocessing import StandardScaler


def load_data(filename):
    # Define the columns you care about.
    selected_columns = [
        "Memory", "Storage", "Rating", "SellingPrice", "OriginalPrice",
        "Discount", "DiscountPercentage", "SellersAmount", "ScreenSize",
        "BatterySize", "Reviews"
    ]
    df = pd.read_csv(filename)
    data = df[selected_columns].copy()
    # Convert values to numeric and drop rows with missing values.
    for col in selected_columns:
        data[col] = pd.to_numeric(data[col], errors='coerce')
    data.dropna(inplace=True)
    return data


def auto_detect_k(scaled_data, max_k=10, default_k=3):
    """
    Uses the elbow method (with the KneeLocator from kneed) to detect
    the optimal number of clusters. If no 'knee' is detected, returns a default.
    """
    distortions = []
    k_range = range(1, max_k + 1)
    for k in k_range:
        kmeans = KMeans(n_clusters=k, random_state=42)
        kmeans.fit(scaled_data)
        distortions.append(kmeans.inertia_)
    try:
        from kneed import KneeLocator
        kn = KneeLocator(list(k_range), distortions, curve='convex', direction='decreasing')
        optimal_k = kn.knee
        if optimal_k is None:
            optimal_k = default_k
    except ImportError:
        # If kneed is not available, use the default value.
        optimal_k = default_k
    return optimal_k


def main():
    parser = argparse.ArgumentParser(
        description="KMeans clustering on CSV data. If --k is <= 0, the elbow method is used."
    )
    parser.add_argument("--filepath", type=str, required=True,
                        help="Path to the CSV file.")
    parser.add_argument("--k", type=int, default=0,
                        help="Number of clusters. Use a value <= 0 to invoke auto-detection (elbow method).")
    args = parser.parse_args()

    # Load and preprocess data.
    data = load_data(args.filepath)
    scaler = StandardScaler()
    scaled_data = scaler.fit_transform(data)

    # Determine k: if k <= 0, use the elbow method.
    if args.k <= 0:
        k = auto_detect_k(scaled_data)
    else:
        k = args.k

    # Perform KMeans clustering.
    kmeans = KMeans(n_clusters=k, random_state=42)
    clusters = kmeans.fit_predict(scaled_data)

    # Output the cluster assignments as a JSON array.
    print(json.dumps(clusters.tolist()))


if __name__ == "__main__":
    main()