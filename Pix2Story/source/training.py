from training.train_encoder import EncoderTrainer
from training.train_decoder import DecoderTrainer

if __name__ == '__main__':
    EncTrainer = EncoderTrainer()
    EncTrainer.train()
    EncTrainer.generate_table()        
    DecTrainer = DecoderTrainer()
    DecTrainer.train()  