'''
This module for converting model to onnx file.
'''
import torch.onnx

def convert_onnx(model_player, input_size):
    """
    Export model to onnx file
    :param model_player: model player (PPOAgent)
    :param input_size: model input size
    """
    # set the model to inference mode
    model_player.policy_network.train(False)
    model_player.value_network.train(False)

    # create a dummy input tensor
    dummy_input = torch.randn(1, input_size, requires_grad=True, device="cpu")

    # export the model
    torch.onnx.export(model_player.policy_network, dummy_input, "pacman_policy.onnx")
    torch.onnx.export(model_player.value_network, dummy_input, "pacman_value.onnx")
    print('Policy and value model has been converted to ONNX.')

# model_save_P1_name = 'training_model_2P_1_v0.pt'
# path_P1 = f"/content/drive/Shareddrives/ML Final Project/Sorting-Battle-Python/training/model/{model_save_P1_name}"
# model_player1 = torch.load(path_P1)
# convert_onnx(model_player1)

# model_save_P2_name = 'training_model_2P_2_v0.pt'
# path_P2 = f"/content/drive/Shareddrives/ML Final Project/Sorting-Battle-Python/training/model/{model_save_P2_name}"
# model_player2 = torch.load(path_P2)
# convert_onnx(model_player2)