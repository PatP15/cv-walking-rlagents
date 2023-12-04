
import os 

from ultralytics import YOLO

# Sets the path to the terrain data directory
data_directory = r'C:\Users\User\OneDrive\Documents\GitHub\cv-walking-rlagents\Assets\Snapshots\terrain_data'

# Load a model
model = YOLO("yolov8n-cls.pt")  # load a pretained model

# Use the model
results = model.train(data=data_directory, epochs=50, imgsz=(960,480))  # train the model