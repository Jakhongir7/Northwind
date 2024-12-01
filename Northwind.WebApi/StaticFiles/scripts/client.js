const apiBaseUrl = "/api/";

async function fetchCategories() {
    const categoriesList = document.getElementById("categories-list");
    categoriesList.innerHTML = "Loading...";
    try {
        const response = await fetch(`${apiBaseUrl}categories`);
        if (response.ok) {
            const categories = await response.json();
            categoriesList.innerHTML = ""; // Clear loading text
            categories.forEach(category => {
                const li = document.createElement("li");
                li.textContent = `${category.categoryID}: ${category.categoryName} - ${category.description}`;
                categoriesList.appendChild(li);
            });
        } else {
            categoriesList.innerHTML = "Failed to load categories.";
        }
    } catch (error) {
        categoriesList.innerHTML = "Error fetching categories.";
        console.error(error);
    }
}

async function fetchProducts() {
    const productsList = document.getElementById("products-list");
    productsList.innerHTML = "Loading...";
    try {
        const response = await fetch(`${apiBaseUrl}products`);
        if (response.ok) {
            const products = await response.json();
            productsList.innerHTML = ""; // Clear loading text
            products.forEach(product => {
                const li = document.createElement("li");
                li.textContent = `${product.productID}: ${product.productName} - $${product.unitPrice || "N/A"} - ${product.discontinued ? "Discontinued" : "Available"}`;
                productsList.appendChild(li);
            });
        } else {
            productsList.innerHTML = "Failed to load products.";
        }
    } catch (error) {
        productsList.innerHTML = "Error fetching products.";
        console.error(error);
    }
}

// Fetch data on page load
fetchCategories();
fetchProducts();
