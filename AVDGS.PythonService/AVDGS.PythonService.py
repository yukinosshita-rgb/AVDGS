from app import app

if __name__ == "__main__":
    # Run Flask service
    app.run(host="0.0.0.0", port=5001, debug=False, threaded=True)
    6