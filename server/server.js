const express = require("express");
const cors = require("cors");
const jwt = require("jsonwebtoken");

const app = express();
app.use(cors());
app.use(express.json());

const users = { "admin": "password123" }; // Simulazione di un database

app.post("/login", (req, res) => {
    const { username, password } = req.body;

    if (users[username] && users[username] === password) {
        const token = jwt.sign({ username }, "secretKey", { expiresIn: "1h" });
        return res.json({ success: true, token });
    }
    
    res.status(401).json({ success: false, message: "Invalid credentials" });
});

app.listen(3000, () => console.log("Server running on port 3000"));
