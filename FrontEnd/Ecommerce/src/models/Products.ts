export interface Product {
  id?: number;
  productCode: string;
  name: string;
  category: string;
  image?: string;
  price: number;
  minimumQuantity: number;
  discountRate: number;
  createdAt?: Date;
  updatedAt?: Date;
}

export interface ProductResponse {
  items: Product[];
  totalCount: number;
  pageSize: number;
  currentPage: number;
  totalPages: number;
}
