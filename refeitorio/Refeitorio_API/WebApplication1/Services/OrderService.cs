using WebApplication1.DTOs;
using WebApplication1.Models;
using WebApplication1.Repositories;

namespace WebApplication1.Services
{
    public class OrderService
    {
        private readonly OrderRepository _orderRepository;
        private readonly CartRepository _cartRepository;
        private readonly ProductRepository _productRepository;
        private readonly UserRepository _userRepository;

        public OrderService(
            OrderRepository orderRepository,
            CartRepository cartRepository,
            ProductRepository productRepository,
            UserRepository userRepository)
        {
            _orderRepository = orderRepository;
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<OrderDTO>> GetAllAsync(int? userId = null, string status = null)
        {
            var orders = await _orderRepository.GetAllAsync(userId, status);
            return orders.Select(MapToDTO);
        }

        public async Task<OrderDTO> GetByIdAsync(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
                return null;

            return MapToDTO(order);
        }

        public async Task<IEnumerable<OrderDTO>> GetUserOrdersAsync(int userId)
        {
            var orders = await _orderRepository.GetAllAsync(userId);
            return orders.Select(MapToDTO);
        }

        public async Task<OrderDTO> CreateFromCartAsync(int userId)
        {
            // Verificar se o usuário existe
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return null;

            // Obter itens do carrinho
            var cartItems = await _cartRepository.GetUserCartAsync(userId);
            if (cartItems == null || !cartItems.Any())
                return null;

            // Calcular o valor total
            decimal totalAmount = cartItems.Sum(item => item.Product.Price * item.Quantity);

            // Verificar se o usuário tem saldo suficiente
            if (user.Balance < totalAmount)
                return null;

            // Debitar o valor do saldo do usuário
            user.Balance -= totalAmount;
            await _userRepository.UpdateAsync(user);

            // Criar o pedido
            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.Now,
                TotalAmount = totalAmount,
                Status = "Pending",
                Items = new List<OrderItem>()
            };

            // Adicionar os itens do pedido
            foreach (var cartItem in cartItems)
            {
                order.Items.Add(new OrderItem
                {
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                    UnitPrice = cartItem.Product.Price
                });

                // Atualizar o estoque do produto
                var product = await _productRepository.GetByIdAsync(cartItem.ProductId);
                if (product != null)
                {
                    product.Stock -= cartItem.Quantity;
                    await _productRepository.UpdateAsync(product);
                }
            }

            // Salvar o pedido
            await _orderRepository.AddAsync(order);

            // Limpar o carrinho do usuário
            await _cartRepository.ClearCartAsync(userId);

            // Retornar o pedido criado
            return await GetByIdAsync(order.Id);
        }

        public async Task<bool> UpdateStatusAsync(int id, string status, int? staffId = null)
        {
            Console.WriteLine($"[API] UpdateStatusAsync - OrderId: {id}, Status: {status}, StaffId: {staffId}");
            
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
            {
                Console.WriteLine($"[API] Order {id} not found");
                return false;
            }

            Console.WriteLine($"[API] Order found - Current Status: {order.Status}, UserId: {order.UserId}, TotalAmount: {order.TotalAmount}");

            // Validar o status
            if (!IsValidStatus(status))
            {
                Console.WriteLine($"[API] Invalid status: {status}");
                return false;
            }

            // Validar transição de estado
            if (!IsValidTransition(order.Status, status))
            {
                Console.WriteLine($"[API] Invalid transition from {order.Status} to {status}");
                return false;
            }

            // Se estiver finalizando a entrega, verificar se o funcionário existe
            if (status == "Delivered" && !staffId.HasValue)
            {
                Console.WriteLine($"[API] Delivered status requires staffId");
                return false;
            }

            // Se estiver cancelando, devolver o saldo ao usuário
            if (status == "Canceled" && order.Status == "Pending")
            {
                Console.WriteLine($"[API] Canceling order - refunding balance to user {order.UserId}");
                var user = await _userRepository.GetByIdAsync(order.UserId);
                if (user != null)
                {
                    var oldBalance = user.Balance;
                    user.Balance += order.TotalAmount;
                    Console.WriteLine($"[API] User balance: {oldBalance} -> {user.Balance}");
                    await _userRepository.UpdateAsync(user);
                    Console.WriteLine($"[API] User balance updated successfully");
                }
                else
                {
                    Console.WriteLine($"[API] User {order.UserId} not found!");
                }
            }

            // Atualizar o pedido
            order.Status = status;
            
            if (status == "Delivered")
            {
                order.DeliveredAt = DateTime.Now;
                order.DeliveredByStaffId = staffId;
            }
            else if (status == "Canceled")
            {
                order.DeliveredAt = DateTime.Now;
            }

            await _orderRepository.UpdateAsync(order);
            return true;
        }

        private bool IsValidStatus(string status)
        {
            return status == "Pending" || 
                   status == "Ready" || 
                   status == "Delivered" ||
                   status == "Canceled";
        }

        private bool IsValidTransition(string currentStatus, string newStatus)
        {
            // Transições válidas:
            // Pending -> Ready (aprovar pedido)
            // Pending -> Canceled (cancelar pedido)
            // Ready -> Delivered (entregar pedido)
            // Ready -> Canceled (cancelar após aprovação - excepcional)
            
            return (currentStatus, newStatus) switch
            {
                ("Pending", "Ready") => true,
                ("Pending", "Canceled") => true,
                ("Ready", "Delivered") => true,
                ("Ready", "Canceled") => true,
                _ => false
            };
        }

        private OrderDTO MapToDTO(Order order)
        {
            return new OrderDTO
            {
                Id = order.Id,
                UserId = order.UserId,
                UserName = order.User?.Name,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                DeliveredAt = order.DeliveredAt,
                Items = order.Items?.Select(item => new OrderItemDTO
                {
                    Id = item.Id,
                    ProductId = item.ProductId,
                    ProductName = item.Product?.Name,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.UnitPrice * item.Quantity
                }).ToList() ?? new List<OrderItemDTO>()
            };
        }
    }
}
